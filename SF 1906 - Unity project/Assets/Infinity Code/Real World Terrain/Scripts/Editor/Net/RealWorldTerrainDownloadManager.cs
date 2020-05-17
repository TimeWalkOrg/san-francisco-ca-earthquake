/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;
using UnityEngine.Networking;

namespace InfinityCode.RealWorldTerrain.Net
{
    public static class RealWorldTerrainDownloadManager
    {
        private const int maxDownloadItem = 16;

        public static long completeSize;
        public static bool finish;

        private static List<RealWorldTerrainDownloadItem> activeItems;
        private static List<RealWorldTerrainDownloadItem> items;
        private static long totalSize;

        public static int count
        {
            get
            {
                if (items == null) return -1;
                return items.Count;
            }
        }

        public static double progress
        {
            get
            {
                if (activeItems == null || activeItems.Count == 0) return 0;
                double localProgress = activeItems.Sum(i =>
                {
                    if (i.ignoreRequestProgress) return 0;
                    return (double) i.progress * i.averageSize;
                }) / totalSize;
                double totalProgress = completeSize / (double)totalSize + localProgress;
                return totalProgress;
            }
        }

        public static int totalSizeMB
        {
            get { return Mathf.RoundToInt(totalSize / (float)RealWorldTerrainUtils.MB); }
        }

        public static void Add(RealWorldTerrainDownloadItem item)
        {
            if (items == null) items = new List<RealWorldTerrainDownloadItem>();
            items.Add(item);
        }

        public static void CheckComplete()
        {
            foreach (RealWorldTerrainDownloadItem item in activeItems) item.CheckComplete();
            if (!RealWorldTerrainWindow.isCapturing) return;

            activeItems.RemoveAll(i => i.complete);
            while (activeItems.Count < maxDownloadItem && items.Count > 0)
            {
                if (!StartNextDownload()) break;
            }
            if (activeItems.Count == 0 && items.Count == 0) finish = true;
        }

        public static void Dispose()
        {
            if (activeItems != null)
            {
                foreach (RealWorldTerrainDownloadItem item in activeItems) item.Dispose();
                activeItems = null;
            }

            items = null;
        }

        public static string EscapeURL(string url)
        {
#if UNITY_2018_3_OR_NEWER
            return UnityWebRequest.EscapeURL(url);
#else
            return WWW.EscapeURL(url);
#endif
        }

        public static void Start()
        {
            finish = false;
            completeSize = 0;

            if (items == null || items.Count == 0)
            {
                finish = true;
                return;
            }

            activeItems = new List<RealWorldTerrainDownloadItem>();

            try
            {
                totalSize = items.Sum(i => i.averageSize);
            }
            catch
            {
                RealWorldTerrainWindow.isCapturing = false;
                Dispose();
                return;
            }

            CheckComplete();
        }

        public static bool StartNextDownload()
        {
            if (items == null || items.Count == 0) return false;

            int index = 0;
            RealWorldTerrainDownloadItem dItem = null;
            while (index < items.Count)
            {
                RealWorldTerrainDownloadItem item = items[index];
                if (item.exclusiveLock != null)
                {
                    bool hasAnotherSomeRequest = false;
                    foreach (RealWorldTerrainDownloadItem activeItem in activeItems)
                    {
                        if (item.exclusiveLock == activeItem.exclusiveLock)
                        {
                            hasAnotherSomeRequest = true;
                            break;
                        }
                    }
                    if (!hasAnotherSomeRequest)
                    {
                        dItem = item;
                        break;
                    }
                }
                else
                {
                    dItem = item;
                    break;
                }
                index++;
            }

            if (dItem == null) return false;

            items.RemoveAt(index);
            if (dItem.exists) return true;
            dItem.Start();
            activeItems.Add(dItem);
            return true;
        }
    }
}