/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainRAWGenerator
    {
        private static int lastX;
        private static short[,] heightmap;

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

            int width = prefs.rawWidth;
            int height = prefs.rawHeight;

            double thx = (mx2 - mx1) / (width - 1);
            double thy = (my2 - my1) / (height - 1);

            if (heightmap == null)
            {
                heightmap = new short[height, width];
                lastX = 0;
            }

            for (int hx = lastX; hx < width; hx++)
            {
                double px = hx * thx + mx1;

                for (int hy = 0; hy < height; hy++)
                {
                    double py = hy * thy + my1;

                    double elevation = RealWorldTerrainElevationGenerator.GetElevation(px, py);
                    int chy = height - hy - 1;

                    if (Math.Abs(elevation - double.MinValue) > double.Epsilon) heightmap[chy, hx] = (short)Math.Round(elevation);
                    else heightmap[chy, hx] = prefs.nodataValue;
                }
                lastX = hx + 1;
                RealWorldTerrainPhase.progress = hx / (float)width;
                if (timer.seconds > 1) return;
            }

            FileInfo info = new FileInfo(prefs.rawFilename);
            if (!Directory.Exists(info.DirectoryName)) Directory.CreateDirectory(info.DirectoryName);

            if (prefs.rawType == RealWorldTerrainRawType.RAW) SaveRAW(prefs);
            else SaveMapboxRGB(prefs);

            heightmap = null;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static void SaveMapboxRGB(RealWorldTerrainPrefs prefs)
        {
            Color[] colors = new Color[prefs.rawWidth * prefs.rawHeight];

            int width = prefs.rawWidth;
            int height = prefs.rawHeight;

            for (int hy = 0; hy < height; hy++)
            {
                int row = hy * width;

                for (int hx = 0; hx < width; hx++)
                {
                    int h = heightmap[hy, hx] + 10000;
                    h *= 10;

                    int b = h % 256;
                    h /= 256;
                    int g = h % 256;
                    h /= 256;
                    int r = h % 256;

                    Color clr = new Color32((byte)r, (byte)g, (byte)b, 1);
                    colors[row + hx] = clr;
                }
            }

            Texture2D texture = new Texture2D(prefs.rawWidth, prefs.rawHeight, TextureFormat.RGB24, false);
            texture.SetPixels(colors);
            texture.Apply();


            string filename = prefs.rawFilename;
            if (!filename.ToLower().EndsWith(".png")) filename += ".png";

            File.WriteAllBytes(filename, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            EditorUtility.RevealInFinder(filename);
        }

        private static void SaveRAW(RealWorldTerrainPrefs prefs)
        {
            string filename = prefs.rawFilename;

            if (!filename.ToLower().EndsWith(".raw")) filename += ".raw";

            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            int width = prefs.rawWidth;
            int height = prefs.rawHeight;

            for (int hy = 0; hy < height; hy++)
            {
                for (int hx = 0; hx < width; hx++)
                {
                    short v = heightmap[hy, hx];
                    if (prefs.rawByteOrder == RealWorldTerrainByteOrder.Windows) writer.Write(v);
                    else
                    {
                        writer.Write((byte)(v / 256));
                        writer.Write((byte)(v % 256));
                    }
                }
            }

            stream.Close();
            EditorUtility.RevealInFinder(filename);
        }
    }
}