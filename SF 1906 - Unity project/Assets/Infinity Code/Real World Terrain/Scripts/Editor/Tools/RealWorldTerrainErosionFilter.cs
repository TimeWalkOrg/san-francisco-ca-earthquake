/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainErosionFilter : EditorWindow
    {
        struct Neighbour
        {
            public float decline;
            public int x;
            public int y;
        }

        private RealWorldTerrainMonoBase target;
        private int iterations = 15;
        private float rainfall = 1.0f;
        private float coneThreshold = 2.0f;
        private int flowIterations = 100;
        private Vector2 scrollPosition;

        private Neighbour[] neighbours = new Neighbour[8];
        private float[,] heightmap;
        private float[,] watermap;
        private float[,] sedimentmap;
        private int width;
        private int height;

        private void AddSediment(int x, int y)
        {
            float v = sedimentmap[y, x];
            float sign = Mathf.Sign(v);
            float absVal = Mathf.Abs(v);
            float threshold = 2e-4f * coneThreshold;

            if (absVal < threshold)
            {
                heightmap[y, x] += sedimentmap[y, x];
                return;
            }

            float radius = absVal / threshold;
            radius = Mathf.Sqrt(radius);
            int r = Mathf.CeilToInt(radius) - 1;

            for (int oy = -r; oy <= r; oy++)
            {
                int cy = y + oy;
                if (cy < 0 || cy >= height) continue;

                for (int ox = -r; ox <= r; ox++)
                {
                    int cx = x + ox;
                    if (cx < 0 || cx >= width) continue;
                    float normDiff = 1 - Mathf.Sqrt(oy * oy + ox * ox) / radius;

                    if (normDiff < 0) normDiff = 0;
                    else if (normDiff > 1) normDiff = 1;

                    heightmap[cy, cx] += threshold * sign * normDiff;
                }
            }
        }

        private void Apply()
        {
            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            int heightmapResolution = -1;

            foreach (RealWorldTerrainItem terrain in terrains)
            {
                if (heightmapResolution == -1) heightmapResolution = terrain.terrainData.heightmapResolution;
                else if (heightmapResolution != terrain.terrainData.heightmapResolution)
                {
                    EditorUtility.DisplayDialog("Error", "Terrains have different heightmap resolution.", "OK");
                    return;
                }
            }

            width = cx * heightmapResolution;
            height = cy * heightmapResolution;

            heightmap = new float[height, width];
            sedimentmap = new float[height, width];
            watermap = new float[height, width];

            float[,] heights;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int tIndex = y * cx + x;
                    heights = terrains[tIndex].terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        int row = y * heightmapResolution + dy;
                        
                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            heightmap[row, x * heightmapResolution + dx] = heights[dy, dx];
                        }
                    }
                }
            }

            if (!WaterErosion()) return;

            heights = new float[heightmapResolution, heightmapResolution];

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        int row = y * heightmapResolution + dy;

                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            heights[dy, dx] = heightmap[row, x * heightmapResolution + dx];
                        }
                    }

                    int tIndex = y * cx + x;
                    terrains[tIndex].terrainData.SetHeights(0, 0, heights);
                }
            }
        }

        private void MoveWater(int x, int y)
        {
            if (watermap[y, x] <= 0) return;

            int cn = 0;
            float sd = 0;
            float v = heightmap[y, x];

            int x1 = Mathf.Max(x - 1, 0);
            int x2 = Mathf.Min(x + 2, width);

            for (int cy = Mathf.Max(y - 1, 0); cy < Mathf.Min(y + 2, height); cy++)
            {
                for (int cx = x1; cx < x2; cx++)
                {
                    if (cy == y && cx == x) continue;

                    float decline = v - heightmap[cy, cx];
                    float lenCoeff = 0.714f;

                    if (decline > 0)
                    {
                        float d = decline * lenCoeff;
                        sd += d;
                        neighbours[cn].decline = d;
                        neighbours[cn].x = cx;
                        neighbours[cn].y = cy;
                        cn++;
                    }
                }
            }

            if (cn > 0)
            {
                if (sd < 0.00001f) sd = 0.00001f;

                float waterNorm = 1 / sd;
                float waterHere = watermap[y, x];

                for (int i = 0; i < cn; i++)
                {
                    Neighbour n = neighbours[i];
                    float waterFlow = waterHere * n.decline * waterNorm;
                    float sandAmount = 0.05f * waterFlow * n.decline;
                    watermap[n.y, n.x] += waterFlow;
                    sedimentmap[y, x] -= sandAmount;
                    sedimentmap[n.y, n.x] += sandAmount;
                }
            }
            watermap[y, x] = 0;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            target = EditorGUILayout.ObjectField("Target", target, typeof(RealWorldTerrainMonoBase), true) as RealWorldTerrainMonoBase;

            iterations = EditorGUILayout.IntField("Erosion iterations", iterations);
            rainfall = EditorGUILayout.FloatField("Rain fall", rainfall);
            coneThreshold = EditorGUILayout.FloatField("Cone threshold", coneThreshold);
            flowIterations = EditorGUILayout.IntField("Flow iterations", flowIterations);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply")) Apply();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            RealWorldTerrainErosionFilter wnd = GetWindow<RealWorldTerrainErosionFilter>(true, "Erosion Filter");
            wnd.target = target;
        }

        private bool WaterErosion()
        {
            for (int i = 0; i < iterations; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Applying erosion", "Please wait...", (float) i / iterations))
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        watermap[y, x] = rainfall;
                        sedimentmap[y, x] = 0;
                    }
                }

                for (int j = 0; j < flowIterations; j++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            MoveWater(x, y);
                        }
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        AddSediment(x, y);
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            return true;
        }
    }
}