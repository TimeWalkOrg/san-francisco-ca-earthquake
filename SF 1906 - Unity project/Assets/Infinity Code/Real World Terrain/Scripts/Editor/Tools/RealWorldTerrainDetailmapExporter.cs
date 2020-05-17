/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainDetailmapExporter:EditorWindow
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
            if (GUILayout.Button("Export"))
            {
                ExportDetailmap();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
        }

        private void ExportDetailmap()
        {
            string filename = EditorUtility.SaveFilePanel("Export RAW Detail map", Application.dataPath, String.Format("detailmap-layer-{0}.raw", layer), "raw");
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
                    EditorUtility.DisplayDialog("Error", "Terrains have different detail map resolution.", "OK");
                    return;
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Create);

            int textureWidth = cx * detailResolution;
            int textureHeight = cy * detailResolution;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    int[,] detailLayer = terrains[tIndex].terrainData.GetDetailLayer(0, 0, detailResolution, detailResolution, layer - 1);

                    for (int dy = 0; dy < detailResolution; dy++)
                    {
                        float progress = ((y * cx + x) * detailResolution + dy) / (float) (cx * cy * detailResolution);
                        EditorUtility.DisplayProgressBar("Export RAW Detail map", Mathf.RoundToInt(progress * 100) + "%", progress);

                        int row = textureHeight - y * detailResolution - dy - 1;
                        int seek = (row * textureWidth + x * detailResolution) * 3;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < detailResolution; dx++)
                        {
                            byte v = (byte)detailLayer[dy, dx];
                            stream.WriteByte(v);
                            stream.WriteByte(v);
                            stream.WriteByte(v);
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
            RealWorldTerrainDetailmapExporter wnd = GetWindow<RealWorldTerrainDetailmapExporter>(true, "Detail map Exporter");
            wnd.target = target;
        }
    }
}
