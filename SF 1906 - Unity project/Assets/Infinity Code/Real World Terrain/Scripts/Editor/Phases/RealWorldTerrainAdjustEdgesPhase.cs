/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainAdjustEdgesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Adjust Edges..."; }
        }

        public override void Enter()
        {
            RealWorldTerrainItem[,] items = terrains;

            for (int x = 0; x < prefs.terrainCount.x - 1; x++)
            {
                for (int y = 0; y < prefs.terrainCount.y; y++)
                {
                    float[,] heights1 = items[x, y].terrainData.GetHeights(prefs.heightmapResolution - 1, 0, 1, prefs.heightmapResolution);
                    float[,] heights2 = items[x + 1, y].terrainData.GetHeights(0, 0, 1, prefs.heightmapResolution);

                    for (int i = 0; i < prefs.heightmapResolution; i++) heights1[i, 0] = (heights1[i, 0] + heights2[i, 0]) / 2;

                    items[x, y].terrainData.SetHeights(prefs.heightmapResolution - 1, 0, heights1);
                    items[x + 1, y].terrainData.SetHeights(0, 0, heights1);
                }
            }

            for (int y = 0; y < prefs.terrainCount.y - 1; y++)
            {
                for (int x = 0; x < prefs.terrainCount.x; x++)
                {
                    float[,] heights1 = items[x, y].terrainData.GetHeights(0, prefs.heightmapResolution - 1, prefs.heightmapResolution, 1);
                    float[,] heights2 = items[x, y + 1].terrainData.GetHeights(0, 0, prefs.heightmapResolution, 1);

                    for (int i = 0; i < prefs.heightmapResolution; i++) heights1[0, i] = (heights1[0, i] + heights2[0, i]) / 2;

                    items[x, y].terrainData.SetHeights(0, prefs.heightmapResolution - 1, heights1);
                    items[x, y + 1].terrainData.SetHeights(0, 0, heights1);
                }
            }

            Complete();
        }
    }
}