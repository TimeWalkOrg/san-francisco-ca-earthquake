/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateTreesPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Trees..."; }
        }

        public override void Enter()
        {
            if (prefs.treeEngine == "Standard")
            {
                RealWorldTerrainTreesGenerator.Generate(RealWorldTerrainWindow.container);
            }
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            else if (prefs.treeEngine == "Vegetation Studio")
            {
                RealWorldTerrainVegetationStudioTreesGenerator.Generate(RealWorldTerrainWindow.container);
            }
#endif
            else phaseComplete = true;

            progress = phaseProgress;

            if (phaseComplete) Complete();
        }
    }
}