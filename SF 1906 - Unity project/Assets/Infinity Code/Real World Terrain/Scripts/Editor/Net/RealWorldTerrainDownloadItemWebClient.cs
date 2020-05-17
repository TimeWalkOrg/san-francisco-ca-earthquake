/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Net;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Net
{
    public class RealWorldTerrainDownloadItemWebClient : RealWorldTerrainDownloadItem
    {
        public Action<WebClient> OnPrepare;

        private float _progress;
        private WebClient client;

        public RealWorldTerrainDownloadItemWebClient(string url)
        {
            RealWorldTerrainDownloadManager.Add(this);
            this.url = url;
        }

        public override float progress
        {
            get { return _progress; }
        }

        public override void CheckComplete()
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            client.Dispose();
            client = null;
        }

        private void OnDownloadDataComplete(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debug.LogWarning("Download error: " + url + "\n" + e.Error);
                CreateErrorFile();
                if (OnError != null) OnError(this);
            }
            else
            {
                byte[] data = e.Result;

                if (data.Length != 0)
                {
                    SaveWWWData(data);
                    DispatchCompete(ref data);
                }
                else CreateErrorFile();
            }

            RealWorldTerrainDownloadManager.completeSize += averageSize;
            _progress = 1;
            complete = true;
        }

        private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _progress = e.ProgressPercentage / 100f;
        }

        public override void Start()
        {
            client = new WebClient();
            client.DownloadDataCompleted += OnDownloadDataComplete;
            client.DownloadProgressChanged += OnProgressChanged;

            if (OnPrepare != null) OnPrepare(client);

            client.DownloadDataAsync(new Uri(url));
        }
    }
}