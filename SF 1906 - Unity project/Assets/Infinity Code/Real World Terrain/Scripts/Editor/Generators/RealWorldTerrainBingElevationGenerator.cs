/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainBingElevationGenerator : RealWorldTerrainElevationGenerator
    {
        public static string key;

        private static string bingFolder;

        private string filename;

        private double mx1;
        private double mx2;
        private double my1;
        private double my2;
        private double tx1;
        private double tx2;
        private double ty1;
        private double ty2;
        private static byte[] readBuffer;

        private RealWorldTerrainBingElevationGenerator(double x1, double y1, double x2, double y2, double tx1, double ty1, double tx2, double ty2)
        {
            this.tx1 = tx1;
            this.ty1 = ty1;
            this.tx2 = tx2;
            this.ty2 = ty2;
            RealWorldTerrainUtils.LatLongToMercat(x1, y1, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(x2, y2, out mx2, out my2);

            filename = Path.Combine(
                RealWorldTerrainEditorUtils.heightmapCacheFolder, 
                String.Format(RealWorldTerrainCultureInfo.numberFormat, "bing_{0}x{1}x{2}x{3}x{4}.rwt", x1, y1, x2, y2, mapSize));
            if (!File.Exists(filename)) Download();
        }

        public override bool Contains(double x, double y)
        {
            return true;
        }

        private void Download()
        {
            int s = mapSize / 32;
            double sizeX = (tx2 - tx1) / s;
            double sizeY = (ty2 - ty1) / s;

            for (int x = 0; x < s; x++)
            {
                double tsx = sizeX * x + tx1;
                double tex = tsx + sizeX;
                tex = (tex - tsx) / 33 * 32 + tsx;

                for (int y = 0; y < s; y++)
                {
                    double tsy = sizeY * y + ty1;
                    double tey = tsy + sizeY;
                    tey = (tey - tsy) / 33 * 32 + tsy;

                    double sx, sy, ex, ey;
                    RealWorldTerrainUtils.TileToLatLong(tsx, tsy, 20, out sx, out sy);
                    RealWorldTerrainUtils.TileToLatLong(tex, tey, 20, out ex, out ey);

                    string partFilename = Path.Combine(bingFolder, string.Format(RealWorldTerrainCultureInfo.numberFormat, "bing_{0}x{1}x{2}x{3}x{4}.json", sx, sy, ex, ey, mapSize));

                    if (!File.Exists(partFilename))
                    {
                        string partURL = string.Format(RealWorldTerrainCultureInfo.numberFormat, "https://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds={0},{1},{2},{3}&rows=32&cols=32&key={4}", ey, sx, sy, ex, key);
                        new RealWorldTerrainDownloadItemWebClient(partURL)
                        {
                            filename = partFilename,
                            averageSize = 5200
                        };
                    }
                }
            }
        }

        public new static void Dispose()
        {
            if (elevations != null)
            {
                foreach (RealWorldTerrainElevationGenerator elevation in elevations) elevation.heightmap = null;
            }
            elevations = null;
            readBuffer = null;
        }

        public static void GetBingElevationRange(out double minEl, out double maxEl)
        {
            minEl = double.MaxValue;
            maxEl = double.MinValue;

            foreach (RealWorldTerrainBingElevationGenerator elevation in elevations)
            {
                foreach (short h in elevation.heightmap)
                {
                    if (h < minEl) minEl = h;
                    if (h > maxEl) maxEl = h;
                }
            }

            if (prefs.generateUnderWater && minEl > prefs.nodataValue) minEl = prefs.nodataValue;
        }

        public override double GetElevationValue(double x, double y)
        {
            double ex = (x - mx1) / (mx2 - mx1);
            double ey = 1 - (my2 - y) / (my2 - my1);

            double cx = ex * mapSize2;
            double cy = ey * mapSize2;

            int ix = (int)cx;
            int iy = (int)cy;

            if (prefs.generateUnderWater && prefs.nodataValue != 0 && GetValue(ix, iy) <= 0) return double.MinValue;

            double ox = cx - ix;
            double oy = cy - iy;

            double e = GetSmoothElevation(ox - 1, ox - 2, ox + 1, oy - 1, oy - 2, oy + 1, ix, iy, ox, ix + 1, oy, iy + 1, ox * oy, ix - 1, iy - 1, ix + 2, iy + 2);
            return e;
        }

        public static void Init()
        {
            mapSize = prefs.heightmapResolution - 1;

            double sx = prefs.leftLongitude;
            double sy = prefs.topLatitude;
            double ex = prefs.rightLongitude;
            double ey = prefs.bottomLatitude;

            double dx, dy;
            RealWorldTerrainUtils.DistanceBetweenPoints(sx, sy, ex, ey, out dx, out dy);

            while (mapSize > 32 && dx / mapSize < 0.005 && dy / mapSize < 0.005) mapSize /= 2;
            mapSize2 = mapSize - 1;

            bingFolder = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, "BingParts");
            if (!Directory.Exists(bingFolder)) Directory.CreateDirectory(bingFolder);

            double stx, sty, etx, ety;
            RealWorldTerrainUtils.LatLongToTile(sx, sy, 20, out stx, out sty);
            RealWorldTerrainUtils.LatLongToTile(ex, ey, 20, out etx, out ety);

            double rx = (etx - stx) / 2;
            double ry = (ety - sty) / 2;

            double ctx = stx + rx;
            double cty = sty + ry;

            rx *= 1.1;
            ry *= 1.1;

            stx = ctx - rx;
            sty = cty - ry;
            etx = ctx + rx;
            ety = cty + ry;

            elevations = new List<RealWorldTerrainElevationGenerator>();

            double cx1, cy1, cx2, cy2;
            RealWorldTerrainUtils.TileToLatLong(stx, sty, 20, out cx1, out cy1);
            RealWorldTerrainUtils.TileToLatLong(etx, ety, 20, out cx2, out cy2);

            double rangeX = prefs.rightLongitude - prefs.leftLongitude;

            double dsx = rangeX / (prefs.heightmapResolution - 1) * (Math.Abs(prefs.depthSharpness) > float.Epsilon ? prefs.depthSharpness : 1);
            depthStep = (float)dsx * 100;

            elevations.Add(new RealWorldTerrainBingElevationGenerator(cx1, cy1, cx2, cy2, stx, sty, etx, ety));
        }

        public static bool Load()
        {
            for (int i = 0; i < elevations.Count; i++)
            {
                RealWorldTerrainBingElevationGenerator elevation = (RealWorldTerrainBingElevationGenerator)elevations[i];
                RealWorldTerrainPhase.phaseProgress = i / (float)elevations.Count;
                if (File.Exists(elevation.filename)) elevation.LoadHeightmap();
                else if (!elevation.TryLoadParts())
                {
                    Debug.LogError("Cant load elevation data");
                    return false;
                }
            }
            return true;
        }

        private void LoadHeightmap()
        {
            heightmap = new short[mapSize, mapSize];
            FileStream fs = File.OpenRead(filename);
            const int size = 32768;
            int c = 0;
            RealWorldTerrainPhase.phaseProgress = 0;
            bool bingMapsUseZeroAsUnknown = prefs.bingMapsUseZeroAsUnknown;
            short x = 0, y = 0;
            if (readBuffer == null) readBuffer = new byte[size];
            do
            {
                int count = fs.Read(readBuffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    short v = (short) (readBuffer[i] | (readBuffer[i + 1] << 8));
                    if (bingMapsUseZeroAsUnknown && v == 0) v = short.MinValue;
                    heightmap[x, y] = v;
                    x++;
                    if (x == mapSize)
                    {
                        x = 0;
                        y++;
                    }
                    c++;
                }
                if (!RealWorldTerrainWindow.isCapturing)
                {
                    fs.Close();
                    return;
                }
            }
            while (fs.Position != fs.Length);

            fs.Close();
            GC.Collect();
        }

        private bool TryLoadParts()
        {
            int s = mapSize / 32;
            double sizeX = (tx2 - tx1) / s;
            double sizeY = (ty2 - ty1) / s;
            heightmap = new short[mapSize, mapSize];

            List<string> filenames = new List<string>();

            for (int x = 0; x < s; x++)
            {
                double tsx = sizeX * x + tx1;
                double tex = tsx + sizeX;
                tex = (tex - tsx) / 33 * 32 + tsx;

                for (int y = 0; y < s; y++)
                {
                    double tsy = sizeY * y + ty1;
                    double tey = tsy + sizeY;
                    tey = (tey - tsy) / 33 * 32 + tsy;

                    double sx, sy, ex, ey;
                    RealWorldTerrainUtils.TileToLatLong(tsx, tsy, 20, out sx, out sy);
                    RealWorldTerrainUtils.TileToLatLong(tex, tey, 20, out ex, out ey);

                    string partFilename = Path.Combine(bingFolder, String.Format(RealWorldTerrainCultureInfo.numberFormat, "bing_{0}x{1}x{2}x{3}x{4}.json", sx, sy, ex, ey, mapSize));

                    if (!File.Exists(partFilename)) return false;

                    filenames.Add(partFilename);
                    string json = File.ReadAllText(partFilename);

                    string startStr = "\"elevations\":[";
                    int startIndex = json.IndexOf(startStr);
                    if (startIndex != -1)
                    {
                        int index = 0;
                        int v = 0;
                        bool isNegative = false;

                        int fx, fy;
                        short sv;

                        for (int i = startIndex + startStr.Length; i < json.Length; i++)
                        {
                            char c = json[i];
                            if (c == ',')
                            {
                                fx = index % 32;
                                fy = 31 - index / 32;
                                if (isNegative) v = -v;
                                sv = (short) v;
                                heightmap[x * 32 + fx, y * 32 + fy] = sv;
                                isNegative = false;
                                v = 0;
                                index++;
                            }
                            else if (c == '-') isNegative = true;
                            else if (c > 47 && c < 58) v = v * 10 + (c - 48);
                            else break;
                        }

                        fx = index % 32;
                        fy = 31 - index / 32;
                        if (isNegative) v = -v;
                        sv = (short) v;
                        heightmap[x * 32 + fx, y * 32 + fy] = sv;
                    }
                    else
                    {
                        Debug.Log("cannot find elevations");
                        return false;
                    }
                }
            }

            FileStream wfs = File.OpenWrite(filename);
            BinaryWriter bw = new BinaryWriter(wfs);

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++) bw.Write(heightmap[x, y]);
            }

            bw.Close();
            wfs.Close();

            foreach (string fn in filenames) File.Delete(fn);
            return true;
        }
    }
}