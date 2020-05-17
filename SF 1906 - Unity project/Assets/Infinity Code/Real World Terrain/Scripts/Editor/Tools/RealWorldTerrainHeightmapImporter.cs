/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainHeightmapImporter : EditorWindow
    {
        private RealWorldTerrainMonoBase target;
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

            depth = EditorGUILayout.IntPopup(new GUIContent("Depth"), depth, depthLabels, depthValues);
            order = (RealWorldTerrainByteOrder)EditorGUILayout.EnumPopup("Byte Order", order);

            bool disabled = target == null || target.prefs.resultType != RealWorldTerrainResultType.terrain;

            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Import")) ImportHeightmap();

            EditorGUI.EndDisabledGroup();
        }

        private void ImportHeightmap()
        {
            string filename = EditorUtility.OpenFilePanel("Import RAW Heightmap", Application.dataPath, "raw");
            if (string.IsNullOrEmpty(filename)) return;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            long fileSize = new FileInfo(filename).Length;

            if (depth == 16) fileSize /= 2;
            fileSize /= cx * cy;

            int heightmapResolution = (int)Mathf.Sqrt(fileSize);
            if (heightmapResolution * heightmapResolution != fileSize)
            {
                EditorUtility.DisplayDialog("Error", "Invalid file size.", "OK");
                return;
            }
            if (Mathf.ClosestPowerOfTwo(heightmapResolution) != heightmapResolution - 1)
            {
                EditorUtility.DisplayDialog("Error", "Invalid file size.", "OK");
                return;
            }

            int textureWidth = cx * heightmapResolution;
            int coof = depth == 8 ? 1 : 2;

            FileStream stream = new FileStream(filename, FileMode.Open);

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    float[,] heightmap = new float[heightmapResolution, heightmapResolution];

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        float progress = ((y * cx + x) * heightmapResolution + dy) / (float)(cx * cy * heightmapResolution);
                        EditorUtility.DisplayProgressBar("Import RAW Heightmap", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = cy * heightmapResolution - (y * heightmapResolution + dy) - 1;
                        int seek = (row * textureWidth + x * heightmapResolution) * coof;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            if (depth == 8) heightmap[dy, dx] = stream.ReadByte() / 256f;
                            else
                            {
                                int b1 = stream.ReadByte();
                                int b2 = stream.ReadByte();

                                if (order == RealWorldTerrainByteOrder.Windows) heightmap[dy, dx] = (b2 * 256 + b1) / 65536f;
                                else heightmap[dy, dx] = (b1 * 256 + b2) / 65536f;
                            }
                        }
                    }

                    int tIndex = y * cx + x;
                    if (terrains[tIndex].terrainData.heightmapResolution != heightmapResolution)
                    {
                        Vector3 size = terrains[tIndex].terrainData.size;
                        terrains[tIndex].terrainData.heightmapResolution = heightmapResolution;
                        terrains[tIndex].terrainData.size = size;
                    }
                    terrains[tIndex].terrainData.SetHeights(0, 0, heightmap);
                }
            }

            stream.Close();
            EditorUtility.ClearProgressBar();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            RealWorldTerrainHeightmapImporter wnd = GetWindow<RealWorldTerrainHeightmapImporter>(true, "Heightmap Importer");
            wnd.target = target;
        }
    }
}
