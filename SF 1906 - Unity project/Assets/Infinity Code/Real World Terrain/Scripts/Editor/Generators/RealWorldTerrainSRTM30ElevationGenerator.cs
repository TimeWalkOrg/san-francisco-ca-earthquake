/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using InfinityCode.Zip;
using UnityEngine;
using UnityEngine.Networking;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainSRTM30ElevationGenerator : RealWorldTerrainElevationGenerator
    {
        private const string server = "https://e4ftl01.cr.usgs.gov/MODV6_Dal_D/SRTM/SRTMGL1.003/2000.02.11/";
        private static List<RealWorldTerrainDownloadItemUnityWebRequest> downloaderItems;

        public static string login;
        public static string pass;

        private readonly string filename;
        private readonly string arcFilename;
        private readonly string heightmapFilename;
        private readonly int x1;
        private readonly int y1;
        private readonly int x2;
        private readonly int y2;
        private double mx1;
        private double mx2;
        private double my1;
        private double my2;
        private byte[] heightmapArchive;
        private static object exclusiveLock;
        
        public RealWorldTerrainSRTM30ElevationGenerator(int X, int Y, ref bool needAuth)
        {
            x1 = X;
            x2 = X + 1;

            y1 = Y + 1;
            y2 = Y;

            mapSize = 3601;
            mapSize2 = mapSize - 1;

            RealWorldTerrainUtils.LatLongToMercat(x1, y1, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(x2, y2, out mx2, out my2);

            string ax = x1 < 0 ? "W" : "E";
            int absX = Mathf.Abs(x1);
            if (absX < 100) ax += "0";
            if (absX < 10) ax += "0";
            ax += absX;

            string ay = y2 < 0 ? "S" : "N";
            int absY = Mathf.Abs(y2);
            if (absY < 10) ay += "0";
            ay += absY;

            filename = String.Format("{1}{0}.hgt", ax, ay);

            arcFilename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, filename + ".zip");
            heightmapFilename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, filename);

            string url = server + ay.ToUpper() + ax.ToUpper() + ".SRTMGL1.hgt.zip";

            if (exclusiveLock == null) exclusiveLock = new object();

            if (!File.Exists(heightmapFilename) && !File.Exists(arcFilename))
            {
                if (needAuth)
                {
                    RealWorldTerrainDownloadItemAction authItem = new RealWorldTerrainDownloadItemAction
                    {
                        exclusiveLock = exclusiveLock
                    };
                    authItem["url"] = url;
                    authItem["step"] = 1;
                    authItem.OnStart = OnAuthStep;
                    authItem.OnCheckComplete = OnAuthCheckComplete;
                    downloaderItems = new List<RealWorldTerrainDownloadItemUnityWebRequest>();
                    needAuth = false;
                }

                RealWorldTerrainDownloadItemUnityWebRequest item = new RealWorldTerrainDownloadItemUnityWebRequest(url)
                {
                    filename = arcFilename,
                    averageSize = 10600000,
                    exclusiveLock = exclusiveLock
                };
                downloaderItems.Add(item);
                item.OnError += OnDownloadError;
            }
        }

        public override bool Contains(double x, double y)
        {
            return x >= mx1 && x <= mx2 && y >= my1 && y <= my2;
        }

        public new static void Dispose()
        {
            if (elevations != null)
            {
                foreach (RealWorldTerrainSRTM30ElevationGenerator elevation in elevations)
                {
                    if (File.Exists(elevation.heightmapFilename))
                    {
                        FileInfo info = new FileInfo(elevation.heightmapFilename);
                        if (info.Length == 0) RealWorldTerrainUtils.SafeDeleteFile(elevation.heightmapFilename);
                    }

                    elevation.heightmap = null;
                }
            }

            lastX = 0;

            lastElevation = null;
        }

        public override double GetElevationValue(double x, double y)
        {
            RealWorldTerrainUtils.MercatToLatLong(x, y, out x, out y);

            x = x - x1;
            y = y1 - y;

            double cx = x * mapSize2;
            double cy = y * mapSize2;

            int ix = (int)cx;
            int iy = (int)cy;

            if (prefs.nodataValue != 0 && GetValue(ix, iy) == short.MinValue) return double.MinValue;

            double ox = cx - ix;
            double oy = cy - iy;

            return GetSmoothElevation(ox - 1, ox - 2, ox + 1, oy - 1, oy - 2, oy + 1, ix, iy, ox, ix + 1, oy, iy + 1, ox * oy, ix - 1, iy - 1, ix + 2, iy + 2);
        }

        public static void GetSRTMElevationRange(out double minEl, out double maxEl)
        {
            minEl = double.MaxValue;
            maxEl = double.MinValue;

            int cx = prefs.terrainCount.x * (prefs.heightmapResolution - 1) + 1;
            int cy = prefs.terrainCount.y * (prefs.heightmapResolution - 1) + 1;

            const int maxV = 4097;
            if (cx > maxV && cx > cy)
            {
                float sv = maxV / (float)cx;
                cx = maxV;
                cy = Mathf.RoundToInt(cy * sv);
            }
            else if (cy > maxV)
            {
                float sv = maxV / (float)cy;
                cy = maxV;
                cx = Mathf.RoundToInt(cx * sv);
            }

            double rx = (prefs.rightLongitude - prefs.leftLongitude) / cx;
            double ry = (prefs.topLatitude - prefs.bottomLatitude) / cy;

            for (int x = 0; x < cx; x++)
            {
                double tx = x * rx + prefs.leftLongitude;

                for (int y = 0; y < cy; y++)
                {
                    double ty = y * ry + prefs.bottomLatitude;
                    double mx, my;
                    RealWorldTerrainUtils.LatLongToMercat(tx, ty, out mx, out my);

                    double el = GetElevation(mx, my);

                    if (el != double.MinValue)
                    {
                        if (el < minEl) minEl = el;
                        if (el > maxEl) maxEl = el;
                    }
                }
            }

            if (minEl > prefs.nodataValue) minEl = prefs.nodataValue;
        }

        protected override short GetValue(int x, int y)
        {
            if (x < 0) x = 0;
            else if (x > mapSize2) x = mapSize2;
            if (y < 0) y = 0;
            else if (y > mapSize2) y = mapSize2;
            short v = heightmap[x, y];
            return v;
        }

        public static void Init()
        {
            int sx = (int)Math.Floor(prefs.leftLongitude + 180) - 180;
            int sy = (int)Math.Floor(prefs.topLatitude + 90) - 90;
            int ex = (int)Math.Floor(prefs.rightLongitude + 180) - 180;
            int ey = (int)Math.Floor(prefs.bottomLatitude + 90) - 90;

            bool needAuth = true;

            for (int x = sx; x <= ex; x++)
            {
                for (int y = sy; y >= ey; y--)
                {
                    elevations.Add(new RealWorldTerrainSRTM30ElevationGenerator(x, y, ref needAuth));
                }
            }
        }

        private void OnAuthCheckComplete(RealWorldTerrainDownloadItemAction action)
        {
            UnityWebRequest uwr = action.GetField<UnityWebRequest>("uwr");
            if (!uwr.isDone) return;

            if (uwr.responseCode != 302)
            {
                Debug.LogError("Authorization error on usgs.gov. Please check your login and password.");
                RealWorldTerrainWindow.CancelCapture();
                return;
            }

            Dictionary<string, string> headers = uwr.GetResponseHeaders();

            int step = action.GetField<int>("step");
            if (step < 3)
            {
                action["url"] = headers["Location"];
                action["step"] = step + 1;
                OnAuthStep(action);
            }
            else
            {
                foreach (RealWorldTerrainDownloadItemUnityWebRequest item in downloaderItems)
                {
                    item.headers = new Dictionary<string, string>();
                    item.headers.Add("Cookie", headers["Set-Cookie"]);
                }
                action.Dispose();
                action.complete = true;
            }
        }

        private void OnAuthStep(RealWorldTerrainDownloadItemAction action)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(action.GetField<string>("url"));
            string auth = login.Trim() + ":" + pass.Trim();
            auth = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            uwr.SetRequestHeader("AUTHORIZATION", auth);
            uwr.redirectLimit = 0;
            action["uwr"] = uwr;
#if UNITY_2018_2_OR_NEWER
            uwr.SendWebRequest();
#else
            uwr.Send();
#endif
        }

        private void OnDownloadError(RealWorldTerrainDownloadItem item)
        {
            if (!prefs.ignoreSRTMErrors) RealWorldTerrainWindow.CancelInMainThread();
        }

        public void ParseHeightmap()
        {
            heightmap = new short[mapSize, mapSize];

            if (!File.Exists(heightmapFilename))
            {
                if (prefs.ignoreSRTMErrors)
                {
                    for (int i = 0; i < mapSize; i++)
                    {
                        for (int j = 0; j < mapSize; j++)
                        {
                            heightmap[i, j] = short.MinValue;
                        }
                    }
                }
                else RealWorldTerrainWindow.CancelInMainThread();
                return;
            }

            FileStream fs = File.OpenRead(heightmapFilename);
            const int size = 1000000;
            int c = 0;
            RealWorldTerrainPhase.phaseProgress = 0;
            do
            {
                byte[] buffer = new byte[size];
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    short s = (short)(buffer[i] * 256 + buffer[i + 1]);
                    if (s == 0) s = short.MinValue;
                    heightmap[c % mapSize, c / mapSize] = s;
                    c++;
                }
                if (!RealWorldTerrainWindow.isCapturing)
                {
                    fs.Close();
                    return;
                }
                RealWorldTerrainPhase.phaseProgress = c / fs.Length * 2;
            }
            while (fs.Position != fs.Length);

            fs.Close();
            GC.Collect();
        }

        public override void UnzipHeightmap()
        {
            if (File.Exists(heightmapFilename))
            {
                unziped = true;
                return;
            }

            if (!File.Exists(arcFilename))
            {
                if (!prefs.ignoreSRTMErrors) Debug.Log("Can not find the file:" + arcFilename);
                unziped = true;
                return;
            }

            if (new FileInfo(arcFilename).Length == 0)
            {
                RealWorldTerrainUtils.SafeDeleteFile(arcFilename);

                RealWorldTerrainWindow.CancelCapture();
                Debug.LogWarning("Error downloading elevation map.");
                return;
            }

            string localFN = filename;

            Stream baseStream;
            if (heightmapArchive == null) baseStream = File.Open(arcFilename, FileMode.Open);
            else baseStream = new MemoryStream(heightmapArchive);
            ZipInputStream stream = new ZipInputStream(baseStream);

            ZipEntry entry;

            while ((entry = stream.GetNextEntry()) != null)
            {
                if (entry.Name == localFN)
                {
                    byte[] buffer = new byte[entry.Size];
                    stream.Read(buffer, (int)entry.Offset, (int)entry.Size);
                    File.WriteAllBytes(heightmapFilename, buffer);

                    stream.Close();
                    heightmapArchive = null;
                    unziped = true;
                    GC.Collect();
                    return;
                }
            }

            Debug.Log("Unzip failed. Try to re-download elevation map using Real World Terrain Helper.");
        }
    }
}