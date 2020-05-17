/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public static class RealWorldTerrainUpdateNeighbors
    {
        public static void Update(RealWorldTerrainContainer container)
        {
            RealWorldTerrainVector2i terrainCount = container.terrainCount;
            RealWorldTerrainItem[] terrains = container.terrains;
            for (int x = 0; x < terrainCount.x; x++)
            {
                for (int y = 0; y < terrainCount.y; y++)
                {
                    int index = y * terrainCount.x + x;
                    Terrain bottom = y > 0 ? terrains[index - terrainCount.x].terrain : null;
                    Terrain top = y < terrainCount.y - 1 ? terrains[index + terrainCount.x].terrain : null;
                    Terrain left = x > 0 ? terrains[index - 1].terrain : null;
                    Terrain right = x < terrainCount.x - 1 ? terrains[index + 1].terrain : null;
                    terrains[index].terrain.SetNeighbors(left, top, right, bottom);
                }
            }

            foreach (RealWorldTerrainItem terrain in terrains) terrain.terrain.Flush();
        }
    }
}