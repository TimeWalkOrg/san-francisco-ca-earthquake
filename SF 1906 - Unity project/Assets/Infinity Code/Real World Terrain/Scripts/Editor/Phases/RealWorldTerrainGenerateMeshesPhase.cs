/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;
using UnityEditor;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateMeshesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate meshes..."; }
        }

        public override void Enter()
        {
            if (index >= terrainCount)
            {
                Complete();
                return;
            }

            int x = index % prefs.terrainCount.x;
            int y = index / prefs.terrainCount.x;

            progress = (index + phaseProgress) / terrainCount;

            RealWorldTerrainMeshGenerator.GenerateMesh(terrains[x, y]);

            if (phaseComplete)
            {
                index++;
                phaseProgress = 0;
                phaseComplete = false;
            }
        }

        public override void Finish()
        {
            AssetDatabase.Refresh();
        }
    }
}