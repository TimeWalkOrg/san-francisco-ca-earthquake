/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateTexturesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Textures..."; }
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

            RealWorldTerrainTextureGenerator.GenerateTexture(terrains[x, y]);

            if (phaseComplete)
            {
                index++;
                phaseProgress = 0;
                phaseComplete = false;
            }
        }

        public override void Finish()
        {
            RealWorldTerrainTextureGenerator.colors = null;
        }
    }
}