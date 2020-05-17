/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainGrassGeneratorWindow : EditorWindow
    {
        private static RealWorldTerrainGrassGeneratorWindow wnd;

        private List<RealWorldTerrainColorItem> colors;
        private Color dryColor = new Color32(205, 188, 26, 255);
        private Color healthyColor = new Color32(67, 249, 42, 255);
        private RealWorldTerrainItem item;
        private int maxHeight = 2;
        private int maxWidth = 2;
        private int minHeight = 1;
        private int minWidth = 1;
        private float noiseSpread = 0.1f;
        private Texture2D previewTexture;
        private Vector2 scrollPosition;
        private Texture2D texture;

        private void DrawPreview()
        {
            TerrainData tdata = item.terrainData;
            Texture2D originalTexture = item.texture;
            float[,,] alphamap = tdata.GetAlphamaps(0, 0, tdata.alphamapWidth, tdata.alphamapHeight);
            Color[] originalColors = originalTexture.GetPixels();
            int w = originalTexture.width;
            int h = originalTexture.height;
            float sw = w / (float)tdata.alphamapWidth;
            float sh = h / (float)tdata.alphamapHeight;
            int l = alphamap.GetLength(2) - 1;
            float step = 1 / (sw * sh);

            for (int x = 0; x < w; x++)
            {
                int fx = Mathf.FloorToInt(x / sw);
                bool isFirstX = x % sw == 0;
                for (int y = 0; y < h; y++)
                {
                    int fy = Mathf.FloorToInt(y / sh);
                    bool isFirstY = y % sh == 0;
                    Color clr = originalColors[x * w + y];
                    if (isFirstX && isFirstY) alphamap[fx, fy, l] = 0;
                    alphamap[fx, fy, l] += colors.Any(c => c.EqualWithRange(clr)) ? step : 0;
                }
            }
            tdata.SetAlphamaps(0, 0, alphamap);
        }

        private void ExportSettings()
        {
            string path = EditorUtility.SaveFilePanel("Export settings", Application.dataPath, "GrassGeneratorSettings.xml", "xml");
            if (string.IsNullOrEmpty(path)) return;

            XmlDocument doc = new XmlDocument();
            XmlElement firstElement = (XmlElement)doc.AppendChild(doc.CreateElement("GrassGenerator"));
            firstElement.SetAttribute("dryColor", RealWorldTerrainUtils.ColorToHex(dryColor));
            firstElement.SetAttribute("healthyColor", RealWorldTerrainUtils.ColorToHex(healthyColor));
            firstElement.SetAttribute("minWidth", minWidth.ToString());
            firstElement.SetAttribute("maxWidth", maxWidth.ToString());
            firstElement.SetAttribute("minHeight", minHeight.ToString());
            firstElement.SetAttribute("maxHeight", maxHeight.ToString());
            firstElement.SetAttribute("noiseSpread", noiseSpread.ToString());
            firstElement.SetAttribute("textureID", (texture != null) ? texture.GetInstanceID().ToString() : "-1");

            foreach (RealWorldTerrainColorItem color in colors) firstElement.AppendChild(color.GetNode(doc));

            doc.Save(path);
        }

        private void GenerateGrass()
        {
            DetailPrototype prototype = new DetailPrototype
            {
                prototypeTexture = texture,
                renderMode = DetailRenderMode.GrassBillboard,
                minWidth = minWidth,
                minHeight = minHeight,
                maxHeight = maxHeight,
                maxWidth = maxWidth,
                dryColor = dryColor,
                healthyColor = healthyColor,
                noiseSpread = noiseSpread
            };

            TerrainData tdata = item.terrainData;
            Texture2D originalTexture = item.texture;
            int[,] detailmap = new int[tdata.detailResolution, tdata.detailResolution];
            Color[] originalColors = originalTexture.GetPixels();
            int w = originalTexture.width;
            int h = originalTexture.height;
            float sw = w / (float)tdata.detailResolution;
            float sh = h / (float)tdata.detailResolution;

            for (int x = 0; x < w; x++)
            {
                int fx = Mathf.FloorToInt(x / sw);
                bool isFirstX = x % sw == 0;
                for (int y = 0; y < h; y++)
                {
                    int fy = Mathf.FloorToInt(y / sh);
                    bool isFirstY = y % sh == 0;
                    Color clr = originalColors[x * w + y];
                    if (isFirstX && isFirstY) detailmap[fx, fy] = 0;
                    detailmap[fx, fy] += colors.Any(c => c.EqualWithRange(clr)) ? 1 : 0;
                }
            }

            List<DetailPrototype> prototypes = tdata.detailPrototypes.ToList();
            prototypes.Add(prototype);
            tdata.detailPrototypes = prototypes.ToArray();
            tdata.SetDetailLayer(0, 0, tdata.detailPrototypes.Length - 1, detailmap);
        }

        private void GeneratePreview()
        {
            TerrainData tdata = item.terrainData;

            RealWorldTerrainEditorUtils.GeneratePreviewTexture(tdata, ref previewTexture);

            DrawPreview();
            GC.Collect();
        }

        private void ImportSettings()
        {
            string path = EditorUtility.OpenFilePanel("Export settings", Application.dataPath, "xml");
            if (string.IsNullOrEmpty(path)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlElement firstElement = (XmlElement)doc.FirstChild;
            if (firstElement.Name != "GrassGenerator") return;

            dryColor = RealWorldTerrainUtils.HexToColor(firstElement.GetAttribute("dryColor"));
            healthyColor = RealWorldTerrainUtils.HexToColor(firstElement.GetAttribute("healthyColor"));
            minWidth = int.Parse(firstElement.GetAttribute("minWidth"));
            maxWidth = int.Parse(firstElement.GetAttribute("maxWidth"));
            minHeight = int.Parse(firstElement.GetAttribute("minHeight"));
            maxHeight = int.Parse(firstElement.GetAttribute("maxHeight"));
            noiseSpread = float.Parse(firstElement.GetAttribute("noiseSpread"));
            int textureID = int.Parse(firstElement.GetAttribute("textureID"));
            if (textureID != -1) texture = (Texture2D)EditorUtility.InstanceIDToObject(textureID);
            else texture = null;

            colors = new List<RealWorldTerrainColorItem>();

            foreach (XmlElement node in firstElement.ChildNodes)
            {
                RealWorldTerrainColorItem color = new RealWorldTerrainColorItem();
                color.SetNode(node);
                colors.Add(color);
            }
        }

        private void OnDestroy()
        {
            OnDisable();
            wnd = null;
        }

        private void OnDisable()
        {
            if (previewTexture != null)
            {
#if UNITY_2018_3_OR_NEWER
                List<TerrainLayer> tls = item.terrainData.terrainLayers.ToList();
                tls.RemoveAll(l => l.diffuseTexture == previewTexture);
                item.terrainData.terrainLayers = tls.ToArray();
#else
                List<SplatPrototype> sps = item.terrainData.splatPrototypes.ToList();
                sps.RemoveAll(sp => sp.texture == previewTexture);
                item.terrainData.splatPrototypes = sps.ToArray();
#endif
                previewTexture = null;
                EditorUtility.UnloadUnusedAssetsImmediate();
            }
        }

        private void OnGUI()
        {
            OnGUIToolbar();

            item = EditorGUILayout.ObjectField("Terrain Item: ", item, typeof(RealWorldTerrainItem), true) as RealWorldTerrainItem;
            if (item == null) return;

            OnGUIProps();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < colors.Count; i++) colors[i].OnGUI(i + 1);
            EditorGUILayout.EndScrollView();

            colors.RemoveAll(c => c.deleted);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add color")) colors.Add(new RealWorldTerrainColorItem());
            if (GUILayout.Button("Generate preview")) GeneratePreview();
            if (GUILayout.Button("Remove preview") && previewTexture != null) OnDisable();

            if (GUILayout.Button("Generate grass"))
            {
                GenerateGrass();
                Close();
                GC.Collect();
            }

            GUILayout.EndHorizontal();
        }

        private void OnGUIProps()
        {
            if (colors == null) colors = new List<RealWorldTerrainColorItem>();
            texture = EditorGUILayout.ObjectField("Grass texture: ", texture, typeof(Texture2D), true) as Texture2D;

            minWidth = EditorGUILayout.IntField("Min width: ", minWidth);
            maxWidth = EditorGUILayout.IntField("Max width: ", maxWidth);
            minHeight = EditorGUILayout.IntField("Min height: ", minHeight);
            maxHeight = EditorGUILayout.IntField("Max height: ", maxHeight);
            noiseSpread = EditorGUILayout.FloatField("Noise spread: ", noiseSpread);
            healthyColor = EditorGUILayout.ColorField("Healthy color: ", healthyColor);
            dryColor = EditorGUILayout.ColorField("Dry color: ", dryColor);
        }

        private void OnGUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUIStyle toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton) { padding = new RectOffset(5, 5, 2, 2) };

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.openIcon, "Import settings"), toolbarButtonStyle,
                GUILayout.ExpandWidth(false)))
                ImportSettings();
            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.saveIcon, "Export settings"), toolbarButtonStyle,
                GUILayout.ExpandWidth(false)))
                ExportSettings();

            GUILayout.Label("", EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
        }

        public static void OpenWindow()
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainGrassGeneratorWindow>("Grass RealWorldTerrainWindow", true);
        }

        public static void OpenWindow(RealWorldTerrainItem item)
        {
            OpenWindow();
            wnd.item = item;
        }
    }
}