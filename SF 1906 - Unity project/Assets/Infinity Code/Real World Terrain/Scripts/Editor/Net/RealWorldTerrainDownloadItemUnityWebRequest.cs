/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace InfinityCode.RealWorldTerrain.Net
{
    public class RealWorldTerrainDownloadItemUnityWebRequest : RealWorldTerrainDownloadItem
    {
        public UnityWebRequest uwr;
        public Dictionary<string, string> headers;

        public RealWorldTerrainDownloadItemUnityWebRequest(string url):this(UnityWebRequest.Get(url))
        {

        }

        public RealWorldTerrainDownloadItemUnityWebRequest(UnityWebRequest uwr)
        {
            RealWorldTerrainDownloadManager.Add(this);

            this.uwr = uwr;
        }

        public override float progress
        {
            get { return uwr.downloadProgress; }
        }

        public override void CheckComplete()
        {
            if (!uwr.isDone) return;

            if (string.IsNullOrEmpty(uwr.error))
            {
                byte[] bytes = uwr.downloadHandler.data;
                SaveWWWData(bytes);
                DispatchCompete(ref bytes);
            }
            else Debug.LogWarning("Download failed: " + uwr.url + "\n" + uwr.error);

            RealWorldTerrainDownloadManager.completeSize += averageSize;
            complete = true;

            Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            uwr.Dispose();
            uwr = null;
        }

        public override void Start()
        {
            if (headers != null)
            {
                foreach (var header in headers) uwr.SetRequestHeader(header.Key, header.Value);
            }
#if UNITY_2018_2_OR_NEWER
            uwr.SendWebRequest();
#else
            uwr.Send();
#endif
        }
    }
}