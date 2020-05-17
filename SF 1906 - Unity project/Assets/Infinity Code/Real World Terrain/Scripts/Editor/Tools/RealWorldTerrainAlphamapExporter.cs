/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainAlphamapExporter : EditorWindow
    {
        private RealWorldTerrainMonoBase target;
        private int layer;
        private int depth = 16;
        private RealWorldTerrainByteOrder order = RealWorldTerrainByteOrder.Windows;
        private GUIContent[] depthLabels;
        private int[] depthValues;

        private void OnEnable()
        {
            depthLabels = new[] { new GUIContent("8"), new GUIContent("16") };
            depthValues = new[] { 8, 16 };
        }

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Target: ", target, typeof(RealWorldTerrainMonoBase), true) as RealWorldTerrainMonoBase;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;
            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            bool disabled = target == null || target.prefs.resultType != RealWorldTerrainResultType.terrain;
            EditorGUI.BeginDisabledGroup(disabled);

            int maxLayer = !disabled ? terrains[0].terrainData.alphamapLayers : 0;
            int minLayer = maxLayer > 0 ? 1 : 0;

            layer = EditorGUILayout.IntSlider("Alphamap Layer", layer, minLayer, maxLayer);

            depth = EditorGUILayout.IntPopup(new GUIContent("Depth"), depth, depthLabels, depthValues);
            order = (RealWorldTerrainByteOrder)EditorGUILayout.EnumPopup("Byte Order", order);

            EditorGUI.BeginDisabledGroup(maxLayer == 0);
            if (GUILayout.Button("Export")) ExportAlphamap();

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        private void ExportAlphamap()
        {
            string filename = EditorUtility.SaveFilePanel("Export RAW Alphamap", Application.dataPath, String.Format("alphamap-layer-{0}.raw", layer), "raw");
            if (string.IsNullOrEmpty(filename)) return;

            int alphaResolution = -1;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            foreach (RealWorldTerrainItem terrain in terrains)
            {
                if (alphaResolution == -1) alphaResolution = terrain.terrainData.alphamapResolution;
                else if (alphaResolution != terrain.terrainData.alphamapResolution)
                {
                    EditorUtility.DisplayDialog("Error", "Terrains have different alphamap resolution.", "OK");
                    return;
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            int textureWidth = cx * alphaResolution;
            int coof = depth == 8 ? 1 : 2;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    float[,,] alpha = terrains[tIndex].terrainData.GetAlphamaps(0, 0, alphaResolution, alphaResolution);

                    for (int dy = 0; dy < alphaResolution; dy++)
                    {
                        float progress = ((y * cx + x) * alphaResolution + dy) / (float)(cx * cy * alphaResolution);
                        EditorUtility.DisplayProgressBar("Export RAW Alphamap", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = y * alphaResolution + dy;
                        int seek = (row * textureWidth + x * alphaResolution) * coof;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < alphaResolution; dx++)
                        {
                            if (depth == 8) writer.Write((byte)Mathf.RoundToInt(alpha[dy, dx, layer - 1] * 255));
                            else
                            {
                                short v = (short)Mathf.RoundToInt(alpha[dy, dx, layer - 1] * 65536);
                                if (order == RealWorldTerrainByteOrder.Windows) writer.Write(v);
                                else
                                {
                                    writer.Write((byte)(v / 256));
                                    writer.Write((byte)(v % 256));
                                }
                            }
                        }
                    }
                }
            }

            stream.Close();
            EditorUtility.ClearProgressBar();

            EditorUtility.RevealInFinder(filename);
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            RealWorldTerrainAlphamapExporter wnd = GetWindow<RealWorldTerrainAlphamapExporter>(true, "Alphamap Exporter");
            wnd.target = target;
        }
    }
}
