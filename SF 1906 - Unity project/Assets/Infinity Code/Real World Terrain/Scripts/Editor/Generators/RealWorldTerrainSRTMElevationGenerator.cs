/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using InfinityCode.Zip;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainSRTMElevationGenerator: RealWorldTerrainElevationGenerator
    {
        private const string server = "http://srtm.csi.cgiar.org/wp-content/uploads/files/srtm_5x5/ASCII/";

        private readonly string arcFilename;
        private readonly string filename;
        private readonly string heightmapFilename;
        private readonly string heightmapFilenameC;
        private readonly int x1;
        private readonly int y1;
        private readonly int x2;
        private readonly int y2;
        private double mx1;
        private double mx2;
        private double my1;
        private double my2;

        private byte[] heightmapArchive;

        public RealWorldTerrainSRTMElevationGenerator(int X, int Y)
        {
            x1 = X;
            y1 = Y + 5;

            x2 = X + 5;
            y2 = Y;

            int ax = Mathf.FloorToInt((X + 180) / 5.0f + 1);
            int ay = Mathf.FloorToInt((90 - Y) / 5.0f - 6);

            RealWorldTerrainUtils.LatLongToMercat(x1, y1, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(x2, y2, out mx2, out my2);

            filename = String.Format("srtm_{0}_{1}", ax > 9 ? ax.ToString() : "0" + ax, ay > 9 ? ay.ToString() : "0" + ay);
            arcFilename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, filename + ".zip");
            heightmapFilename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, filename + ".asc");
            heightmapFilenameC = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, filename + ".rwt");

            if (!File.Exists(heightmapFilenameC) && !File.Exists(heightmapFilename) && !File.Exists(arcFilename))
            {
                new RealWorldTerrainDownloadItemUnityWebRequest(server + filename + ".zip")
                {
                    filename = arcFilename,
                    averageSize = 25000000
                };
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
                foreach (RealWorldTerrainSRTMElevationGenerator elevation in elevations)
                {
                    if (File.Exists(elevation.heightmapFilename))
                    {
                        FileInfo info = new FileInfo(elevation.heightmapFilename);
                        if (info.Length == 0) RealWorldTerrainUtils.SafeDeleteFile(elevation.heightmapFilename);
                    }

                    elevation.heightmap = null;
                    elevation.heightmapArchive = null;
                }
            }

            lastX = 0;
            lastElevation = null;
        }

        public override double GetElevationValue(double x, double y)
        {
            RealWorldTerrainUtils.MercatToLatLong(x, y, out x, out y);

            x = (x - x1) / 5.0f;
            y = (y1 - y) / 5.0f;

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

        public static void Init()
        {
            mapSize = 6000;
            mapSize2 = mapSize - 1;
            int countElX = 0;

            int sx = (int)Math.Floor(prefs.leftLongitude / 5) * 5 + 180;
            int ex = (int)Math.Floor(prefs.rightLongitude / 5) * 5 + 180;
            int sy = 90 - (int)Math.Floor(prefs.topLatitude / 5) * 5;
            int ey = 90 - (int)Math.Floor(prefs.bottomLatitude / 5) * 5;

            for (int x = sx; x <= ex; x += 5)
            {
                countElX++;
                for (int y = sy; y <= ey; y += 5) elevations.Add(new RealWorldTerrainSRTMElevationGenerator(x - 180, 90 - y));
            }
            float dsx = (float)(ex - sx) / countElX / (prefs.heightmapResolution - 1) * (Math.Abs(prefs.depthSharpness) > float.Epsilon ? prefs.depthSharpness : 1);
            depthStep = dsx * 100;
        }

        public void ParseHeightmap()
        {
            if (File.Exists(heightmapFilenameC))
            {
                ParseHeightmapC();
                return;
            }

            heightmap = new short[mapSize, mapSize];

            if (!File.Exists(heightmapFilename))
            {
                for (int hx = 0; hx < mapSize; hx++) for (int hy = 0; hy < mapSize; hy++) heightmap[hx, hy] = short.MinValue;
                if (!prefs.ignoreSRTMErrors) Debug.Log("Can not find the file: " + heightmapFilename);
                return;
            }

            FileStream fs = new FileStream(heightmapFilename, FileMode.Open);
            FileStream wfs = File.OpenWrite(heightmapFilenameC);
            BinaryWriter bw = new BinaryWriter(wfs);

            const int bufferSize = 1024 * 1024;
            int counter = 0;
            short nodata = -9999;
            byte[] buffer = new byte[bufferSize];
            int skipLines = 6;
            bool isNegative = false;
            int v = 0;

            RealWorldTerrainPhase.phaseProgress = 0;
            do
            {
                int readCount = fs.Read(buffer, 0, bufferSize);

                for (int i = 0; i < readCount; i++)
                {
                    byte b = buffer[i];
                    if (skipLines > 0)
                    {
                        if (b == 0x0A)
                        {
                            skipLines--;
                            if (skipLines == 0)
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    b = buffer[j];
                                    if (b != ' ') continue;

                                    for (int k = j + 1; k < i - 1; k++)
                                    {
                                        b = buffer[k];

                                        if (b == '-') isNegative = true;
                                        else v = v * 10 + (b - 0x30);
                                    }

                                    short sv = (short)v;
                                    if (isNegative) sv *= -1;
                                    nodata = sv;

                                    v = 0;
                                    isNegative = false;

                                    break;
                                }
                            }
                        }
                        continue;
                    }

                    if (b == '-') isNegative = true;
                    else if (b == ' ')
                    {
                        int index = counter;
                        short sv = (short) v;
                        if (isNegative) sv *= -1;
                        if (sv == nodata) sv = short.MinValue;
                        heightmap[index % mapSize, index / mapSize] = sv;
                        bw.Write(sv);
                        counter++;

                        v = 0;
                        isNegative = false;
                    }
                    else v = v * 10 + (b - 0x30);
                }

                if (!RealWorldTerrainWindow.isCapturing)
                {
                    fs.Close();
                    bw.Close();
                    RealWorldTerrainUtils.SafeDeleteFile(heightmapFilenameC);
                    return;
                }
                RealWorldTerrainPhase.phaseProgress = fs.Position / (float)fs.Length;
            }
            while (fs.Position != fs.Length);

            fs.Close();
            bw.Close();
        }

        private void ParseHeightmapC()
        {
            heightmap = new short[mapSize, mapSize];
            FileStream fs = File.OpenRead(heightmapFilenameC);
            const int size = 1024 * 1024;
            int c = 0;
            RealWorldTerrainPhase.phaseProgress = 0;
            byte[] buffer = new byte[size];

            do
            {
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    heightmap[c % mapSize, c / mapSize] = BitConverter.ToInt16(buffer, i);
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

            string localFN = filename + ".asc";

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