/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;

namespace InfinityCode.RealWorldTerrain.Net
{
    public class RealWorldTerrainDownloadItemAction : RealWorldTerrainDownloadItem
    {
        public Action<RealWorldTerrainDownloadItemAction> OnStart;
        public Action<RealWorldTerrainDownloadItemAction> OnCheckComplete;

        public override float progress
        {
            get { return 0; }
        }

        public RealWorldTerrainDownloadItemAction()
        {
            RealWorldTerrainDownloadManager.Add(this);
        }

        public override void CheckComplete()
        {
            if (OnCheckComplete != null) OnCheckComplete(this);
            else complete = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            OnStart = null;
        }

        public override void Start()
        {
            if (OnStart != null) OnStart(this);
            else complete = true;
        }
    }
}