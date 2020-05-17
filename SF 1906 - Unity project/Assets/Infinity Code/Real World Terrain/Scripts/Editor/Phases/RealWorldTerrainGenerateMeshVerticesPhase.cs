/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateMeshVerticesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate mesh vertices..."; }
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

            RealWorldTerrainMeshGenerator.GenerateVertices(terrains[x, y]);

            if (phaseComplete)
            {
                index++;
                phaseProgress = 0;
                phaseComplete = false;
            }
        }
    }
}