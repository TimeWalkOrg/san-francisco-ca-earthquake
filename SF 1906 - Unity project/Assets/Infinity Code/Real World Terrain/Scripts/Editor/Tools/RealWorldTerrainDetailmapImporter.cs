/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainDetailmapImporter:EditorWindow
    {
        private RealWorldTerrainMonoBase target;
        private int layer;

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Target: ", target, typeof(RealWorldTerrainMonoBase), true) as RealWorldTerrainMonoBase;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;
            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            bool disabled = target == null || target.prefs.resultType != RealWorldTerrainResultType.terrain;
            EditorGUI.BeginDisabledGroup(disabled);
            int maxLayer = !disabled ? terrains[0].terrainData.detailPrototypes.Length : 0;
            int minLayer = maxLayer > 0 ? 1 : 0;

            layer = EditorGUILayout.IntSlider("Detail layer", layer, minLayer, maxLayer);

            EditorGUI.BeginDisabledGroup(maxLayer == 0);
            if (GUILayout.Button("Import"))
            {
                ImportDetailmap();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        private void ImportDetailmap()
        {
            string filename = EditorUtility.OpenFilePanel("Import RAW Detailmap", Application.dataPath, "raw");
            if (string.IsNullOrEmpty(filename)) return;

            int detailResolution = -1;

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            foreach (RealWorldTerrainItem terrain in terrains)
            {
                if (detailResolution == -1) detailResolution = terrain.terrainData.detailResolution;
                else if (detailResolution != terrain.terrainData.detailResolution)
                {
                    EditorUtility.DisplayDialog("Error", "Terrains have different detailmap resolution.", "OK");
                    return;
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Open);

            int textureWidth = cx * detailResolution;
            int textureHeight = cy * detailResolution;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int[,] detailLayer = new int[detailResolution, detailResolution];

                    for (int dy = 0; dy < detailResolution; dy++)
                    {
                        float progress = ((y * cx + x) * detailResolution + dy) / (float)(cx * cy * detailResolution);
                        EditorUtility.DisplayProgressBar("Import RAW Detailmap", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = textureHeight - y * detailResolution - dy - 1;
                        int seek = (row * textureWidth + x * detailResolution) * 3;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < detailResolution; dx++)
                        {
                            int r = stream.ReadByte();
                            int g = stream.ReadByte();
                            int b = stream.ReadByte();
                            detailLayer[dy, dx] = (r + g + b) / 3;
                        }
                    }

                    int tIndex = y * cx + x;
                    terrains[tIndex].terrainData.SetDetailLayer(0, 0, layer - 1, detailLayer);
                }
            }

            stream.Close();
            EditorUtility.ClearProgressBar();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            RealWorldTerrainDetailmapImporter wnd = GetWindow<RealWorldTerrainDetailmapImporter>(true, "Detailmap Importer");
            wnd.target = target;
        }
    }
}
