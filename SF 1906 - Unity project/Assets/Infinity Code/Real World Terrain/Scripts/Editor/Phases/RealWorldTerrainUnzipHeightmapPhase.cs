/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Threading;
using InfinityCode.RealWorldTerrain.Generators;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainUnzipHeightmapPhase : RealWorldTerrainPhase
    {
        private RealWorldTerrainElevationGenerator activeElevation;

        public override string title
        {
            get { return "Unzip Elevations..."; }
        }

        public override void Enter()
        {
            if (activeElevation != null && !activeElevation.unziped) return;

            if (index >= RealWorldTerrainElevationGenerator.elevations.Count)
            {
                Complete();
                return;
            }
            activeElevation = RealWorldTerrainElevationGenerator.elevations[index];

            progress = index / (float)RealWorldTerrainElevationGenerator.elevations.Count;

            if (activeElevation.unziped)
            {
                index++;
                return;
            }

            RealWorldTerrainElevationGenerator generator = activeElevation;
            if (generateInThread) new Thread(generator.UnzipHeightmap).Start();
            else generator.UnzipHeightmap();
        }

        public override void Finish()
        {
            activeElevation = null;
        }

        public override void Start()
        {
            if (prefs.elevationProvider != RealWorldTerrainElevationProvider.SRTM && 
                prefs.elevationProvider != RealWorldTerrainElevationProvider.SRTM30)
            {
                Complete();
            }
        }
    }
}