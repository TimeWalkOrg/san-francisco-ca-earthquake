/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Diagnostics;
using System.IO;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainGaiaStampGenerator
    {
        private static int lastX;
        private static short[,] heightmap;
        public static string fullFilename;
        public static string shortFilename;
        private static double minEl;
        private static double maxEl;
        private static double elRange;

        public static void Generate()
        {
            RealWorldTerrainPrefs prefs = RealWorldTerrainWindow.prefs;

            double tx = prefs.leftLongitude;
            double ty = prefs.topLatitude;
            double bx = prefs.rightLongitude;
            double by = prefs.bottomLatitude;

            double mx1, my1, mx2, my2;
            RealWorldTerrainUtils.LatLongToMercat(tx, ty, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(bx, by, out mx2, out my2);

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            int res = prefs.gaiaStampResolution;

            double thx = (mx2 - mx1) / (res - 1);
            double thy = (my2 - my1) / (res - 1);

            if (heightmap == null)
            {
                heightmap = new short[res, res];
                lastX = 0;
                RealWorldTerrainElevationGenerator.GetElevationRange(out minEl, out maxEl);
                elRange = (maxEl - minEl) / 32000;
            }

            for (int hx = lastX; hx < res; hx++)
            {
                double px = hx * thx + mx1;

                for (int hy = 0; hy < res; hy++)
                {
                    double py = hy * thy + my1;

                    double elevation = RealWorldTerrainElevationGenerator.GetElevation(px, py);
                    int chy = res - hy - 1;

                    if (Math.Abs(elevation - double.MinValue) > double.Epsilon) heightmap[chy, hx] = (short)Math.Round((elevation - minEl) / elRange);
                    else heightmap[chy, hx] = prefs.nodataValue;
                }
                lastX = hx + 1;
                RealWorldTerrainPhase.progress = hx / (float)res;
                if (timer.seconds > 1) return;
            }

            string resultFolder = "Assets/RWT_Result";
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            resultFolder += "/" + dateStr;
            if (!Directory.Exists(resultFolder)) Directory.CreateDirectory(resultFolder);

            shortFilename = "rwt gaia heightmap " + dateStr;
            fullFilename = Path.Combine(resultFolder, shortFilename + ".raw");
            
            FileStream stream = new FileStream(fullFilename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            for (int hx = 0; hx < res; hx++)
            {
                for (int hy = 0; hy < res; hy++)
                {
                    writer.Write(heightmap[hy, hx]);
                }
            }

            stream.Close();
            heightmap = null;

            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}