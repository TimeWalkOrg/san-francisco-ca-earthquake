/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateRiversPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Rivers..."; }
        }

        public override void Enter()
        {
            RealWorldTerrainRiverGenerator.Generate(RealWorldTerrainWindow.container);

            progress = phaseProgress;
            if (phaseComplete) Complete();
        }
    }
}