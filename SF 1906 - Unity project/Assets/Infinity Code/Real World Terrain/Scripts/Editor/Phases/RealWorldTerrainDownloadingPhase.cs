/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Net;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainDownloadingPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Downloading..."; }
        }

        public override void Enter()
        {
            if (!RealWorldTerrainDownloadManager.finish)
            {
                RealWorldTerrainDownloadManager.CheckComplete();
                progress = (float)RealWorldTerrainDownloadManager.progress;
            }
            else Complete();
        }

        public override void Start()
        {
            RealWorldTerrainDownloadManager.Start();
        }
    }
}