/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainAlphamapImporter : EditorWindow
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

            layer = EditorGUILayout.IntSlider("Alphamap layer", layer, minLayer, maxLayer);

            depth = EditorGUILayout.IntPopup(new GUIContent("Depth"), depth, depthLabels, depthValues);
            order = (RealWorldTerrainByteOrder)EditorGUILayout.EnumPopup("Byte Order", order);

            EditorGUI.BeginDisabledGroup(maxLayer == 0);
            if (GUILayout.Button("Import"))
            {
                ImportAlphamap();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        private void ImportAlphamap()
        {
            string filename = EditorUtility.OpenFilePanel("Import RAW Alphamap", Application.dataPath, "raw");
            if (string.IsNullOrEmpty(filename)) return;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            long fileSize = new FileInfo(filename).Length;

            if (depth == 16) fileSize /= 2;
            fileSize /= cx * cy;

            int alphamapResolution = (int)Mathf.Sqrt(fileSize);
            if (alphamapResolution * alphamapResolution != fileSize)
            {
                EditorUtility.DisplayDialog("Error", "Invalid file size.", "OK");
                return;
            }
            if (Mathf.ClosestPowerOfTwo(alphamapResolution) != alphamapResolution)
            {
                EditorUtility.DisplayDialog("Error", "Invalid file size.", "OK");
                return;
            }

            FileStream stream = new FileStream(filename, FileMode.Open);

            int textureWidth = cx * alphamapResolution;
            int coof = depth == 8 ? 1 : 2;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    float[,,] alphamap = terrains[tIndex].terrainData.GetAlphamaps(0, 0, alphamapResolution, alphamapResolution);
                    int alphaCount = terrains[tIndex].terrainData.alphamapLayers;

                    for (int dy = 0; dy < alphamapResolution; dy++)
                    {
                        float progress = ((y * cx + x) * alphamapResolution + dy) / (float)(cx * cy * alphamapResolution);
                        EditorUtility.DisplayProgressBar("Import RAW Alphamap", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = y * alphamapResolution + dy;
                        int seek = (row * textureWidth + x * alphamapResolution) * coof;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < alphamapResolution; dx++)
                        {
                            float v;
                            if (depth == 8) v = alphamap[dy, dx, layer - 1] = stream.ReadByte() / 256f;
                            else
                            {
                                int b1 = stream.ReadByte();
                                int b2 = stream.ReadByte();

                                if (order == RealWorldTerrainByteOrder.Windows) v = alphamap[dy, dx, layer - 1] = (b2 * 256 + b1) / 65536f;
                                else v = alphamap[dy, dx, layer - 1] = (b1 * 256 + b2) / 65536f;
                            }

                            float other = 1 - v;

                            if (Math.Abs(v - 1) < 0.0001)
                            {
                                for (int l = 0; l < alphaCount; l++)
                                {
                                    if (l != layer - 1)
                                    {
                                        alphamap[dy, dx, l] = 0;
                                    }
                                }
                            }
                            else
                            {
                                float total = 0;
                                for (int l = 0; l < alphaCount; l++)
                                {
                                    if (l == layer - 1) continue;
                                    total += alphamap[dy, dx, l];
                                }

                                if (total > 0)
                                {
                                    float scale = other / total;

                                    for (int l = 0; l < alphaCount; l++)
                                    {
                                        if (l == layer) continue;
                                        alphamap[dy, dx, l] *= scale;
                                    }
                                }
                                else
                                {
                                    for (int l = 0; l < alphaCount; l++)
                                    {
                                        if (l == layer) continue;
                                        alphamap[dy, dx, l] = 1f / (alphaCount - 1);
                                    }
                                }
                            }
                        }
                    }

                    if (terrains[tIndex].terrainData.alphamapResolution != alphamapResolution)
                    {
                        Vector3 size = terrains[tIndex].terrainData.size;
                        terrains[tIndex].terrainData.alphamapResolution = alphamapResolution;
                        terrains[tIndex].terrainData.size = size;
                    }

                    terrains[tIndex].terrainData.SetAlphamaps(0, 0, alphamap);
                }
            }

            stream.Close();
            EditorUtility.ClearProgressBar();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            RealWorldTerrainAlphamapImporter wnd = GetWindow<RealWorldTerrainAlphamapImporter>(true, "Alphamap Importer");
            wnd.target = target;
        }
    }
}
