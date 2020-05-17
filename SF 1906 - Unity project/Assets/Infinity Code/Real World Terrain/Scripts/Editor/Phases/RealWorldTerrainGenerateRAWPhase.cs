/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateRAWPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate RAW..."; }
        }

        public override void Enter()
        {
            RealWorldTerrainRAWGenerator.Generate();
            if (!phaseComplete) return;

            Complete();
        }
    }
}