/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainSplatPrototypeGenerator : EditorWindow
    {
        private static RealWorldTerrainSplatPrototypeGenerator wnd;
        private static Texture2D previewTexture;

        private RealWorldTerrainSplatPrototypeItem baseTexture;
        private RealWorldTerrainItem item;
        private List<RealWorldTerrainSplatPrototypeItem> prototypes;
        private Vector2 scrollPosition;

        private void ExportSettings()
        {
            string path = EditorUtility.SaveFilePanel("Export settings", Application.dataPath,
                "SplatGeneratorSettings.xml", "xml");
            if (string.IsNullOrEmpty(path)) return;

            XmlDocument doc = new XmlDocument();
            XmlElement firstElement = (XmlElement)doc.AppendChild(doc.CreateElement("SplatGenerator"));

            firstElement.AppendChild(baseTexture.GetNode(doc));
            foreach (RealWorldTerrainSplatPrototypeItem sp in prototypes) firstElement.AppendChild(sp.GetNode(doc));

            doc.Save(path);
        }

        public static void GeneratePreview(RealWorldTerrainSplatPrototypeItem sp)
        {
            TerrainData tdata = wnd.item.terrainData;

            RealWorldTerrainEditorUtils.GeneratePreviewTexture(tdata, ref previewTexture);

#if UNITY_2018_3_OR_NEWER
            Texture2D originalTexture = tdata.terrainLayers[0].diffuseTexture;
#else
            Texture2D originalTexture = tdata.splatPrototypes[0].texture;
#endif
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
                    alphamap[fx, fy, l] += sp.colors.Any(c => c.EqualWithRange(clr)) ? step : 0;
                }
            }
            tdata.SetAlphamaps(0, 0, alphamap);
            alphamap = null;
            originalColors = null;
            GC.Collect();
        }

        private void GenerateSplatPrototypes()
        {
            TerrainData tdata = item.terrainData;
#if UNITY_2018_3_OR_NEWER
            List<TerrainLayer> spsList = new List<TerrainLayer>(tdata.terrainLayers);
            int startIndex = spsList.Count;
            int endIndex = startIndex + prototypes.Count + 1;
            spsList.Add(baseTexture.terrainLayer);

            spsList.AddRange(prototypes.Select(prototype => prototype.terrainLayer));
            tdata.terrainLayers = spsList.ToArray();
            tdata.RefreshPrototypes();

            Texture2D originalTexture = tdata.terrainLayers[0].diffuseTexture;
#else
            List<SplatPrototype> spsList = new List<SplatPrototype>(tdata.splatPrototypes);
            int startIndex = spsList.Count;
            int endIndex = startIndex + prototypes.Count + 1;
            spsList.Add(baseTexture.splat);

            spsList.AddRange(prototypes.Select(prototype => prototype.splat));
            tdata.splatPrototypes = spsList.ToArray();
            tdata.RefreshPrototypes();

            Texture2D originalTexture = tdata.splatPrototypes[0].texture;
#endif
            float[,,] alphamap = tdata.GetAlphamaps(0, 0, tdata.alphamapWidth, tdata.alphamapHeight);
            Color[] originalColors = originalTexture.GetPixels();
            int w = originalTexture.width;
            int h = originalTexture.height;
            float sw = w / (float)tdata.alphamapWidth;
            float sh = h / (float)tdata.alphamapHeight;
            float step = 1 / (sw * sh);

            for (int x = 0; x < alphamap.GetLength(0); x++)
            {
                for (int y = 0; y < alphamap.GetLength(1); y++)
                    alphamap[x, y, startIndex] = 1;
            }

            for (int l = startIndex + 1; l < endIndex; l++)
            {
                RealWorldTerrainSplatPrototypeItem prototype = prototypes[l - startIndex - 1];
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
                        alphamap[fx, fy, l] += prototype.colors.Any(c => c.EqualWithRange(clr)) ? step : 0;
                    }
                }
            }

            for (int x = 0; x < alphamap.GetLength(0); x++)
            {
                for (int y = 0; y < alphamap.GetLength(1); y++)
                {
                    float total = 0;
                    for (int l = startIndex + 1; l < endIndex; l++) total += alphamap[x, y, l];
                    for (int l = 0; l < alphamap.GetLength(2); l++)
                    {
                        if (l < startIndex || l >= endIndex) alphamap[x, y, l] = 0;
                        else if (l == startIndex)
                        {
                            if (total < 1) alphamap[x, y, l] = 1 - total;
                            else alphamap[x, y, l] = 0;
                        }
                        else if (total != 0)
                        {
                            if (total < 1)
                                alphamap[x, y, l] = total;
                            else alphamap[x, y, l] /= total;
                        }
                    }
                }
            }

            tdata.SetAlphamaps(0, 0, alphamap);
            alphamap = null;
            originalColors = null;
            GC.Collect();
        }

        private void ImportSettings()
        {
            string path = EditorUtility.OpenFilePanel("Export settings", Application.dataPath, "xml");
            if (string.IsNullOrEmpty(path)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlElement firstElement = (XmlElement)doc.FirstChild;
            if (firstElement.Name != "SplatGenerator") return;

            baseTexture = new RealWorldTerrainSplatPrototypeItem(true);
            baseTexture.SetNode((XmlElement)firstElement.ChildNodes[0]);

            prototypes = new List<RealWorldTerrainSplatPrototypeItem>();

            for (int i = 1; i < firstElement.ChildNodes.Count; i++)
            {
                RealWorldTerrainSplatPrototypeItem sp = new RealWorldTerrainSplatPrototypeItem();
                sp.SetNode((XmlElement)firstElement.ChildNodes[i]);
                prototypes.Add(sp);
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

        private void OnEnable()
        {
            wnd = this;
        }

        private void OnGUI()
        {
            if (baseTexture == null) baseTexture = new RealWorldTerrainSplatPrototypeItem(true);
            if (prototypes == null) prototypes = new List<RealWorldTerrainSplatPrototypeItem>();

            OnGUIToolbar();

            item = EditorGUILayout.ObjectField("Terrain Item: ", item, typeof(RealWorldTerrainItem), true) as RealWorldTerrainItem;
            if (item == null) return;

            baseTexture.OnGUI();
            int index = prototypes.Count;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (RealWorldTerrainSplatPrototypeItem prototype in prototypes) prototype.OnGUI(index--);
            GUILayout.EndScrollView();

            prototypes.RemoveAll(p => p.deleted);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add SplatPrototype"))
                prototypes.Insert(0, new RealWorldTerrainSplatPrototypeItem());

            if (GUILayout.Button("Clear preview"))
                OnDisable();

            if (GUILayout.Button("Generate SplatPrototypes"))
            {
                GenerateSplatPrototypes();
                Close();
            }

            GUILayout.EndHorizontal();
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

            wnd = GetWindow<RealWorldTerrainSplatPrototypeGenerator>("SplatPrototype RealWorldTerrainWindow", true);
        }

        public static void OpenWindow(RealWorldTerrainItem item)
        {
            OpenWindow();
            wnd.item = item;
        }
    }
}