/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using System.Text;
using InfinityCode.RealWorldTerrain.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainMapboxElevationGenerator : RealWorldTerrainElevationGenerator
    {
        public static string key;
        private double mx1;
        private double mx2;
        private double my1;
        private double my2;
        private int zoom;

        private int isx, isy, iex, iey;
        private int mapWidth;
        private int mapHeight;

        private RealWorldTerrainMapboxElevationGenerator(int isx, int isy, int iex, int iey, int zoom)
        {
            this.isx = isx;
            this.isy = isy;
            this.iex = iex;
            this.iey = iey;
            this.zoom = zoom;

            RealWorldTerrainUtils.TileToLatLong(isx, isy, zoom, out mx1, out my1);
            RealWorldTerrainUtils.TileToLatLong(iex + 1, iey + 1, zoom, out mx2, out my2);
            RealWorldTerrainUtils.LatLongToMercat(ref mx1, ref my1);
            RealWorldTerrainUtils.LatLongToMercat(ref mx2, ref my2);

            mapWidth = (iex - isx + 1) * 256;
            mapHeight = (iey - isy + 1) * 256;

            for (int tx = isx; tx <= iex; tx++)
            {
                for (int ty = isy; ty <= iey; ty++)
                {
                    string filename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, zoom + "x" + tx + "x" + ty + ".pngraw");
                    if (!File.Exists(filename))
                    {
                        string url = new StringBuilder("https://api.mapbox.com/v4/mapbox.terrain-rgb/")
                            .Append(zoom).Append("/").Append(tx).Append("/").Append(ty)
                            .Append(".pngraw?access_token=").Append(key).ToString();

                        new RealWorldTerrainDownloadItemUnityWebRequest(url)
                        {
                            filename = filename,
                            averageSize = 25000
                        };
                    }
                }
            }

            
        }

        public override bool Contains(double x, double y)
        {
            return x >= mx1 && x <= mx2 && y >= my1 && y <= my2;
        }

        public override double GetElevationValue(double x, double y)
        {
            double ex = (x - mx1) / (mx2 - mx1);
            double ey = 1 - (my2 - y) / (my2 - my1);

            double cx = ex * mapWidth;
            double cy = ey * mapHeight;

            int ix = (int)cx;
            int iy = (int)cy;

            if (prefs.generateUnderWater && prefs.nodataValue != 0 && GetValue(ix, iy) == 0) return double.MinValue;

            double ox = cx - ix;
            double oy = cy - iy;

            double e = GetSmoothElevation(ox - 1, ox - 2, ox + 1, oy - 1, oy - 2, oy + 1, ix, iy, ox, ix + 1, oy, iy + 1, ox * oy, ix - 1, iy - 1, ix + 2, iy + 2);
            return e;
        }

        public static void GetMapboxElevationRange(out double minEl, out double maxEl)
        {
            minEl = double.MaxValue;
            maxEl = double.MinValue;

            RealWorldTerrainMapboxElevationGenerator elevation = elevations[0] as RealWorldTerrainMapboxElevationGenerator;

            foreach (short h in elevation.heightmap)
            {
                if (h < minEl) minEl = h;
                if (h > maxEl) maxEl = h;
            }

            if (prefs.generateUnderWater && minEl > prefs.nodataValue) minEl = prefs.nodataValue;
        }

        protected override short GetValue(int x, int y)
        {
            if (x < 0) x = 0;
            else if (x >= mapWidth) x = mapWidth - 1;
            if (y < 0) y = 0;
            else if (y >= mapHeight) y = mapHeight - 1;
            short v = heightmap[x, y];
            return v;
        }

        public static void Init()
        {
            double rangeX = prefs.rightLongitude - prefs.leftLongitude;
            double rangeY = prefs.topLatitude - prefs.bottomLatitude;

            double dpa = Mathf.Max(prefs.heightmapResolution * prefs.terrainCount.x, prefs.heightmapResolution * prefs.terrainCount.y) / Math.Max(rangeX, rangeY);
            int zoom = 0;

            int maxzoom = 14;
            for (int i = 5; i < maxzoom; i++)
            {
                float cdpa = 256 * (1 << i) / 360f;
                if (cdpa > dpa)
                {
                    zoom = i;
                    break;
                }
            }

            if (zoom == 0) zoom = maxzoom;

            double stx, sty, etx, ety;
            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, zoom, out stx, out sty);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, zoom, out etx, out ety);

            int isx = (int) stx;
            int isy = (int) sty;
            int iex = (int) etx;
            int iey = (int) ety;

            elevations.Add(new RealWorldTerrainMapboxElevationGenerator(isx, isy, iex, iey, zoom));

            double dsx = rangeX / (prefs.heightmapResolution - 1) * (Math.Abs(prefs.depthSharpness) > float.Epsilon ? prefs.depthSharpness : 1);
            depthStep = (float)dsx * 100;
        }

        public static bool Load()
        {
            return (elevations[0] as RealWorldTerrainMapboxElevationGenerator).TryLoadElevations();
        }

        private bool TryLoadElevations()
        {
            heightmap = new short[mapWidth, mapHeight];
            const int res = 256;

            for (int tx = isx; tx <= iex; tx++)
            {
                int cx = (tx - isx) * res;

                for (int ty = isy; ty <= iey; ty++)
                {
                    int cy = (ty - isy) * res;

                    string filename = Path.Combine(RealWorldTerrainEditorUtils.heightmapCacheFolder, zoom + "x" + tx + "x" + ty + ".pngraw");
                    if (!File.Exists(filename)) return false;
                    byte[] bytes = File.ReadAllBytes(filename);

                    Texture2D texture = new Texture2D(res, res);
                    if (!texture.LoadImage(bytes)) return false;

                    Color[] colors = texture.GetPixels();
                    Object.DestroyImmediate(texture);

                    for (int y = 0; y < res; y++)
                    {
                        int py = (255 - y) * res;
                        int hy = y + cy;

                        for (int x = 0; x < res; x++)
                        {
                            Color c = colors[py + x];

                            double height = -10000 + (c.r * 255 * 256 * 256 + c.g * 255 * 256 + c.b * 255) * 0.1;
                            heightmap[x + cx, hy] = (short)Math.Round(height);
                        }
                    }
                }
            }

            return true;
        }
    }
}