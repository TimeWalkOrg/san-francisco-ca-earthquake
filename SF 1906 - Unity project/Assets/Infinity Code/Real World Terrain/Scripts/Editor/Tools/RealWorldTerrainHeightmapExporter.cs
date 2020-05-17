/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerraiHeightmapExporter : EditorWindow
    {
        private RealWorldTerrainMonoBase target;
        private int depth = 16;
        private RealWorldTerrainByteOrder order = RealWorldTerrainByteOrder.Windows;
        private GUIContent[] depthLabels;
        private int[] depthValues;

        private void OnEnable()
        {
            depthLabels = new[] {new GUIContent("8"), new GUIContent("16")};
            depthValues = new[] { 8, 16 };
        }

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Target", target, typeof(RealWorldTerrainMonoBase), true) as RealWorldTerrainMonoBase;
            depth = EditorGUILayout.IntPopup(new GUIContent("Depth"), depth, depthLabels, depthValues);
            order = (RealWorldTerrainByteOrder) EditorGUILayout.EnumPopup("Byte Order", order);
            bool disabled = target == null || target.prefs.resultType != RealWorldTerrainResultType.terrain;
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Export")) ExportHeightmap();
            EditorGUI.EndDisabledGroup();

            if (target != null)
            {
                RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
                int cx = container != null ? container.terrainCount.x : 1;
                int cy = container != null ? container.terrainCount.y : 1;

                RealWorldTerrainItem item = target as RealWorldTerrainItem;
                RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

                int heightmapResolution = terrains[0].terrainData.heightmapResolution;

                int textureWidth = cx * heightmapResolution;
                int textureHeight = cy * heightmapResolution;
                EditorGUILayout.HelpBox("Width: " + textureWidth + "px, height: " + textureHeight + "px, channel: 1", MessageType.Info);
            }
        }

        private void ExportHeightmap()
        {
            string filename = EditorUtility.SaveFilePanel("Export RAW Heightmap", Application.dataPath, "heightmap.raw", "raw");
            if (string.IsNullOrEmpty(filename)) return;

            int heightmapResolution = -1;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            foreach (RealWorldTerrainItem terrain in terrains)
            {
                if (heightmapResolution == -1) heightmapResolution = terrain.terrainData.heightmapResolution;
                else if (heightmapResolution != terrain.terrainData.heightmapResolution)
                {
                    EditorUtility.DisplayDialog("Error", "Terrains have different heightmap resolution.", "OK");
                    return;
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            int textureWidth = cx * heightmapResolution;
            int coof = depth == 8 ? 1 : 2;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    float[,] heightmap = terrains[tIndex].terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        float progress = (tIndex * heightmapResolution + dy) / (float)(cx * cy * heightmapResolution);
                        EditorUtility.DisplayProgressBar("Export RAW Heightmap", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = cy * heightmapResolution - (y * heightmapResolution + dy) - 1;
                        int seek = (row * textureWidth + x * heightmapResolution) * coof;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            if (depth == 8) writer.Write((byte) Mathf.RoundToInt(heightmap[dy, dx] * 255));
                            else
                            {
                                short v = (short)Mathf.RoundToInt(heightmap[dy, dx] * 65536);
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
            RealWorldTerraiHeightmapExporter wnd = GetWindow<RealWorldTerraiHeightmapExporter>(true, "Heightmap Exporter");
            wnd.target = target;
        }
    }
}
