/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainLoadTerrainsPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Load Terrains..."; }
        }

        public override void Enter()
        {
            RealWorldTerrainMonoBase target = RealWorldTerrainWindow.generateTarget;
            if (target == null) Complete();

            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;

            if (container != null)
            {
                RealWorldTerrainWindow.container = container;
                RealWorldTerrainWindow.terrains = new RealWorldTerrainItem[RealWorldTerrainWindow.container.terrainCount.x, RealWorldTerrainWindow.container.terrainCount.y];
                for (int i = 0; i < container.terrains.Length; i++)
                {
                    int tx = i % container.terrainCount.x;
                    int ty = i / container.terrainCount.x;
                    RealWorldTerrainItem item = terrains[tx, ty] = container.terrains[i];
                    if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain && item.prefs.resultType == RealWorldTerrainResultType.mesh)
                    {
                        item.prefs.heightmapResolution = RealWorldTerrainWindow.prefs.heightmapResolution;
                        item["texture"] = item.texture;
                        while (item.transform.childCount > 0)
                        {
                            Object.DestroyImmediate(item.transform.GetChild(0).gameObject);
                        }
                    }
                }
            }
            else
            {
                RealWorldTerrainItem item = (RealWorldTerrainItem)target;
                RealWorldTerrainWindow.container = item.container;
                RealWorldTerrainWindow.terrains = new RealWorldTerrainItem[1, 1];
                terrains[0, 0] = item;
                if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain && item.prefs.resultType == RealWorldTerrainResultType.mesh)
                {
                    item.prefs.heightmapResolution = RealWorldTerrainWindow.prefs.heightmapResolution;
                    item["texture"] = item.texture;
                    while (item.transform.childCount > 0)
                    {
                        Object.DestroyImmediate(item.transform.GetChild(0).gameObject);
                    }
                }
            }

            Complete();
        }
    }
}