/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateRoadsPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Roads..."; }
        }

        public override void Enter()
        {
            RealWorldTerrainRoadGenerator.Generate(RealWorldTerrainWindow.container);
            progress = phaseProgress;

            if (phaseComplete) Complete();
        }
    }
}