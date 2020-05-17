/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class contains utility methods.
    /// </summary>
    public static class RealWorldTerrainUtils
    {
        /// <summary>
        /// The radius of the Earth.
        /// </summary>
        public const float EARTH_RADIUS = 6371;

        /// <summary>
        /// The length of the equator.
        /// </summary>
        public const int EQUATOR_LENGTH = 40075;

        /// <summary>
        /// The average size of the texture of the tile.
        /// </summary>
        public const int AVERAGE_TEXTURE_SIZE = 20000;

        /// <summary>
        /// Degrees-to-radians conversion constant.
        /// </summary>
        public const double DEG2RAD = Math.PI / 180;

        /// <summary>
        /// Maximum the size of the download for Google Maps.
        /// </summary>
        public const int DOWNLOAD_TEXTURE_LIMIT = 90000000;

        /// <summary>
        /// The maximum elevation in the world.
        /// </summary>
        public const int MAX_ELEVATION = 15000;

        /// <summary>
        /// Size of tile.
        /// </summary>
        public const short TILE_SIZE = 256;

        /// <summary>
        /// The number of bytes in megabyte.
        /// </summary>
        public const int MB = 1048576;

        /// <summary>
        /// PI * 4
        /// </summary>
        public const float PI4 = 4 * Mathf.PI;

        /// <summary>
        /// The angle between the two points in degree.
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <returns>Angle in degree</returns>
        public static float Angle2D(Vector2 point1, Vector2 point2)
        {
            return Mathf.Atan2((point2.y - point1.y), (point2.x - point1.x)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// The angle between the two points in degree.
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <returns>Angle in degree</returns>
        public static float Angle2D(Vector3 point1, Vector3 point2)
        {
            return Mathf.Atan2((point2.z - point1.z), (point2.x - point1.x)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// The angle between the three points in degree.
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <param name="point3">Point 3</param>
        /// <param name="unsigned">Return a positive result.</param>
        /// <returns>Angle in degree</returns>
        public static float Angle2D(Vector3 point1, Vector3 point2, Vector3 point3, bool unsigned = true)
        {
            float angle1 = Angle2D(point1, point2);
            float angle2 = Angle2D(point2, point3);
            float angle = angle1 - angle2;
            if (angle > 180) angle -= 360;
            if (angle < -180) angle += 360;
            if (unsigned) angle = Mathf.Abs(angle);
            return angle;
        }

        /// <summary>
        /// The angle between the two points in radians.
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <param name="offset">Result offset in degrees.</param>
        /// <returns>Angle in radians</returns>
        public static float Angle2DRad(Vector3 point1, Vector3 point2, float offset)
        {
            return Mathf.Atan2((point2.z - point1.z), (point2.x - point1.x)) + offset * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Clamps value between min and max and returns value.
        /// </summary>
        /// <param name="n">Value</param>
        /// <param name="minValue">Min</param>
        /// <param name="maxValue">Max</param>
        /// <returns>Value in the range between the min and max.</returns>
        public static double Clamp(double n, double minValue, double maxValue)
        {
            if (n < minValue) return minValue;
            if (n > maxValue) return maxValue;
            return n;
        }

        /// <summary>
        /// Clamps a value between a minimum double and maximum double value.
        /// </summary>
        /// <param name="n">Value</param>
        /// <param name="minValue">Minimum</param>
        /// <param name="maxValue">Maximum</param>
        /// <returns>Value between a minimum and maximum.</returns>
        public static double Clip(double n, double minValue, double maxValue)
        {
            if (n < minValue) return minValue;
            if (n > maxValue) return maxValue;
            return n;
        }

        /// <summary>
        /// Gets Hex value of Color.
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Hex value</returns>
        public static string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        public static GameObject CreateGameObject(MonoBehaviour parent, string name)
        {
            return CreateGameObject(parent.gameObject, name, Vector3.zero);
        }

        public static GameObject CreateGameObject(GameObject parent, string name)
        {
            return CreateGameObject(parent, name, Vector3.zero);
        }

        public static GameObject CreateGameObject(GameObject parent, string name, Vector3 position)
        {
            GameObject container = new GameObject(name);
            container.transform.parent = parent.transform;
            container.transform.localPosition = position;
            return container;
        }

        public static void DeleteGameObject(Transform current, string name)
        {
            for (int i = current.childCount - 1; i >= 0; i--)
            {
                Transform child = current.GetChild(i);
                if (child.name == name) Object.DestroyImmediate(child.gameObject);
                else DeleteGameObject(child, name);
            }
        }

        /// <summary>
        /// The distance between two geographical coordinates.
        /// </summary>
        /// <param name="point1">Coordinate (X - Lng, Y - Lat)</param>
        /// <param name="point2">Coordinate (X - Lng, Y - Lat)</param>
        /// <returns>Distance (km).</returns>
        public static Vector2 DistanceBetweenPoints(Vector2 point1, Vector2 point2)
        {
            Vector2 range = point1 - point2;

            double scfY = Math.Sin(point1.y * Mathf.Deg2Rad);
            double sctY = Math.Sin(point2.y * Mathf.Deg2Rad);
            double ccfY = Math.Cos(point1.y * Mathf.Deg2Rad);
            double cctY = Math.Cos(point2.y * Mathf.Deg2Rad);
            double cX = Math.Cos(range.x * Mathf.Deg2Rad);
            double sizeX1 = Math.Abs(EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
            double sizeX2 = Math.Abs(EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
            float sizeX = (float)((sizeX1 + sizeX2) / 2.0);
            float sizeY = (float)(EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY));
            return new Vector2(sizeX, sizeY);
        }

        public static void DistanceBetweenPoints(double x1, double y1, double x2, double y2, out double dx, out double dy)
        {
            double rx = x1 - x2;
            double scfY = Math.Sin(y1 * Mathf.Deg2Rad);
            double sctY = Math.Sin(y2 * Mathf.Deg2Rad);
            double ccfY = Math.Cos(y1 * Mathf.Deg2Rad);
            double cctY = Math.Cos(y2 * Mathf.Deg2Rad);
            double cX = Math.Cos(rx * Mathf.Deg2Rad);
            double sizeX1 = Math.Abs(EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
            double sizeX2 = Math.Abs(EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
            dx = (sizeX1 + sizeX2) / 2.0;
            dy = EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY);
        }

        public static void ExportMesh(string filename, params MeshFilter[] mfs)
        {
            StringBuilder builder = new StringBuilder();
            int nextNormalIndex = 0;
            foreach (MeshFilter mf in mfs)
            {
                Mesh m = mf.sharedMesh;
                Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

                builder.Append("g ").Append(mf.name).Append("\n");
                for (int i = 0; i < m.vertices.Length; i++)
                {
                    Vector3 v = m.vertices[i];
                    builder.Append("v ").Append(v.x).Append(" ").Append(v.y).Append(" ").Append(v.z).Append("\n");
                }
                builder.Append("\n");

                for (int i = 0; i < m.normals.Length; i++)
                {
                    Vector3 v = m.normals[i];
                    builder.Append("vn ").Append(v.x).Append(" ").Append(v.y).Append(" ").Append(v.z).Append("\n");
                }
                builder.Append("\n");

                for (int i = 0; i < m.uv.Length; i++)
                {
                    Vector2 v = m.uv[i];
                    builder.Append("vt ").Append(v.x).Append(" ").Append(v.y).Append("\n");
                }

                for (int material = 0; material < m.subMeshCount; material++)
                {
                    builder.Append("\nusemtl ").Append(mats[material].name).Append("\n");
                    builder.Append("usemap ").Append(mats[material].name).Append("\n");

                    int[] triangles = m.GetTriangles(material);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int tni1 = triangles[i] + 1 + nextNormalIndex;
                        int tni2 = triangles[i + 1] + 1 + nextNormalIndex;
                        int tni3 = triangles[i + 2] + 1 + nextNormalIndex;
                        builder.Append("f ").Append(tni1).Append("/").Append(tni1).Append("/").Append(tni1);
                        builder.Append(" ").Append(tni2).Append("/").Append(tni2).Append("/").Append(tni2);
                        builder.Append(" ").Append(tni3).Append("/").Append(tni3).Append("/").Append(tni3).Append("\n");
                    }
                }

                builder.Append("\n");
                nextNormalIndex += m.normals.Length;
            }

#if !UNITY_WEBPLAYER && !NETFX_CORE
            StreamWriter stream = new StreamWriter(filename);
            stream.Write(builder.ToString());
            stream.Close();
#endif
        }

        public static void GetCenterPointAndZoom(double[] positions, out Vector2 center, out int zoom)
        {
            double minX = Single.MaxValue;
            double minY = Single.MaxValue;
            double maxX = Single.MinValue;
            double maxY = Single.MinValue;

            for (int i = 0; i < positions.Length; i += 2)
            {
                double lng = positions[i];
                double lat = positions[i + 1];
                if (lng < minX) minX = lng;
                if (lat < minY) minY = lat;
                if (lng > maxX) maxX = lng;
                if (lat > maxY) maxY = lat;
            }

            double rx = maxX - minX;
            double ry = maxY - minY;
            double cx = rx / 2 + minX;
            double cy = ry / 2 + minY;

            center = new Vector2((float)cx, (float)cy);

            int width = 1024;
            int height = 1024;

            float countX = width / (float)TILE_SIZE / 2;
            float countY = height / (float)TILE_SIZE / 2;

            for (int z = 20; z > 4; z--)
            {
                bool success = true;

                double tcx, tcy;
                LatLongToTile(cx, cy, z, out tcx, out tcy);

                for (int i = 0; i < positions.Length; i += 2)
                {
                    double lng = positions[i];
                    double lat = positions[i + 1];
                    double px, py;
                    LatLongToTile(lng, lat, z, out px, out py);
                    px -= tcx - countX;
                    py -= tcy - countY;

                    if (px < 0 || py < 0 || px > width || py > height)
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    zoom = z;
                    return;
                }
            }

            zoom = 3;
        }

        public static void GetCenterPointAndZoom(Vector2[] positions, out Vector2 center, out int zoom)
        {
            float minX = Single.MaxValue;
            float minY = Single.MaxValue;
            float maxX = Single.MinValue;
            float maxY = Single.MinValue;

            foreach (Vector2 p in positions)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            float rx = maxX - minX;
            float ry = maxY - minY;
            double cx = rx / 2 + minX;
            double cy = ry / 2 + minY;

            center = new Vector2((float)cx, (float)cy);

            int width = 1024;
            int height = 1024;

            float countX = width / (float)TILE_SIZE / 2;
            float countY = height / (float)TILE_SIZE / 2;

            for (int z = 20; z > 4; z--)
            {
                bool success = true;

                double tcx, tcy;
                LatLongToTile(cx, cy, z, out tcx, out tcy);

                foreach (Vector2 pos in positions)
                {
                    double px, py;
                    LatLongToTile(pos.x, pos.y, z, out px, out py);
                    px -= tcx - countX;
                    py -= tcy - countY;

                    if (px < 0 || py < 0 || px > width || py > height)
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                {
                    zoom = z;
                    return;
                }
            }

            zoom = 3;
        }

        public static long GetDirectorySize(DirectoryInfo folder)
        {
#if !UNITY_WEBPLAYER
            return folder.GetFiles().Sum(fi => fi.Length) + folder.GetDirectories().Sum(dir => GetDirectorySize(dir));
#else
            return 0;
#endif
        }

        public static long GetDirectorySize(string folderPath)
        {
            return GetDirectorySize(new DirectoryInfo(folderPath));
        }

        public static long GetDirectorySizeMB(string folderPath)
        {
            return GetDirectorySize(folderPath) / MB;
        }

        public static Vector2 GetIntersectionPointOfTwoLines(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22,
            out int state)
        {
            state = -2;
            Vector2 result = new Vector2();
            float m = ((p22.x - p21.x) * (p11.y - p21.y) - (p22.y - p21.y) * (p11.x - p21.x));
            float n = ((p22.y - p21.y) * (p12.x - p11.x) - (p22.x - p21.x) * (p12.y - p11.y));

            float Ua = m / n;

            if (n == 0 && m != 0) state = -1;
            else if (m == 0 && n == 0) state = 0;
            else
            {
                result.x = p11.x + Ua * (p12.x - p11.x);
                result.y = p11.y + Ua * (p12.y - p11.y);

                if (((result.x >= p11.x || result.x <= p11.x) && (result.x >= p21.x || result.x <= p21.x))
                    && ((result.y >= p11.y || result.y <= p11.y) && (result.y >= p21.y || result.y <= p21.y)))
                    state = 1;
            }
            return result;
        }

        public static Vector2 GetIntersectionPointOfTwoLines(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22,
            out int state)
        {
            return GetIntersectionPointOfTwoLines(new Vector2(p11.x, p11.z), new Vector2(p12.x, p12.z),
                new Vector2(p21.x, p21.z), new Vector2(p22.x, p22.z), out state);
        }

        public static Rect GetRectFromPoints(List<Vector3> points)
        {
            return new Rect
            {
                x = points.Min(p => p.x),
                y = points.Min(p => p.z),
                xMax = points.Max(p => p.x),
                yMax = points.Max(p => p.z)
            };
        }

        public static Color HexToColor(string hex)
        {
            byte r = Byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = Byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = Byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        public static bool IsClockWise(Vector3 A, Vector3 B, Vector3 C)
        {
            return (B.x - A.x) * (C.z - A.z) - (C.x - A.x) * (B.z - A.z) > 0;
        }

        public static bool IsPointInPolygon(Vector3[] poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }

        public static bool IsPointInPolygon(List<Vector3> poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }

        public static bool IsPointInPolygon(List<Vector3> poly, Vector3 point)
        {
            return IsPointInPolygon(poly, point.x, point.z);
        }

        public static Vector2 LanLongToFlat(Vector2 pos)
        {
            return new Vector2(Mathf.FloorToInt(pos.x / 5.0f) * 5 + 180, 90 - Mathf.FloorToInt(pos.y / 5.0f) * 5);
        }

        /// <summary>
        /// Converts geographic coordinates to Mercator coordinates.
        /// </summary>
        /// <param name="x">Longitude</param>
        /// <param name="y">Latitude</param>
        public static void LatLongToMercat(ref double x, ref double y)
        {
            double sy = Math.Sin(y * DEG2RAD);
            x = (x + 180) / 360;
            y = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);
        }

        public static void LatLongToMercat(double x, double y, out double mx, out double my)
        {
            double sy = Math.Sin(y * DEG2RAD);
            mx = (x + 180) / 360;
            my = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);
        }

        /// <summary>
        /// Converts geographic coordinates to the index of the tile.
        /// What is the tiles, and how it works, you can read here:
        /// https://developers.google.com/maps/documentation/javascript/v2/overlays?csw=1#Google_Maps_Coordinates
        /// </summary>
        /// <param name="dx">Longitude</param>
        /// <param name="dy">Latitude</param>
        /// <param name="zoom">Zoom</param>
        /// <param name="tx">Tile X</param>
        /// <param name="ty">Tile Y</param>
        public static void LatLongToTile(double dx, double dy, int zoom, out double tx, out double ty)
        {
            LatLongToMercat(ref dx, ref dy);
            uint mapSize = (uint)TILE_SIZE << zoom;
            double px = Clamp(dx * mapSize + 0.5, 0, mapSize - 1);
            double py = Clamp(dy * mapSize + 0.5, 0, mapSize - 1);
            tx = px / TILE_SIZE;
            ty = py / TILE_SIZE;
        }

        public static int Limit(int val, int min = 32, int max = 4096)
        {
            return Mathf.Clamp(val, min, max);
        }

        public static int LimitPowTwo(int val, int min = 32, int max = 4096)
        {
            return Mathf.Clamp(Mathf.ClosestPowerOfTwo(val), min, max);
        }

        public static void MercatToLatLong(double mx, double my, out double x, out double y)
        {
            uint mapSize = (uint)TILE_SIZE << 20;
            double px = Clamp(mx * mapSize + 0.5, 0, mapSize - 1);
            double py = Clamp(my * mapSize + 0.5, 0, mapSize - 1);
            mx = px / TILE_SIZE;
            my = py / TILE_SIZE;
            TileToLatLong(mx, my, 20, out x, out y);
        }

        public static double Repeat(double n, double minValue, double maxValue)
        {
            if (double.IsInfinity(n) || double.IsInfinity(minValue) || double.IsInfinity(maxValue) || double.IsNaN(n) || double.IsNaN(minValue) || double.IsNaN(maxValue)) return n;

            double range = maxValue - minValue;
            while (n < minValue || n > maxValue)
            {
                if (n < minValue) n += range;
                else if (n > maxValue) n -= range;
            }
            return n;
        }

        public static string ReplaceString(string str, string[] oldValues, string newValue)
        {
            foreach (string oldValue in oldValues) str = str.Replace(oldValue, newValue);
            return str;
        }

        public static string ReplaceString(string str, string[] oldValues, string[] newValues)
        {
            for (int i = 0; i < oldValues.Length; i++) str = str.Replace(oldValues[i], newValues[i]);
            return str;
        }

        public static void SafeDeleteDirectory(string directoryName)
        {
            try
            {
#if !UNITY_WEBPLAYER
                Directory.Delete(directoryName, true);
#endif
            }
            catch
            { }
        }

        public static void SafeDeleteFile(string filename, int tryCount = 10)
        {
            while (tryCount-- > 0)
            {
                try
                {
                    File.Delete(filename);
                    break;
                }
                catch (Exception)
                {
#if !NETFX_CORE
                    Thread.Sleep(10);
#endif
                }
            }
        }

        public static List<T> SpliceList<T>(List<T> list, int offset, int count = 1)
        {
            List<T> newList = list.Skip(offset).Take(count).ToList();
            list.RemoveRange(offset, count);
            return newList;
        }

        public static Color StringToColor(string str)
        {
            str = str.ToLower();
            if (str == "black") return Color.black;
            if (str == "blue") return Color.blue;
            if (str == "cyan") return Color.cyan;
            if (str == "gray") return Color.gray;
            if (str == "green") return Color.green;
            if (str == "magenta") return Color.magenta;
            if (str == "red") return Color.red;
            if (str == "white") return Color.white;
            if (str == "yellow") return Color.yellow;

            try
            {
                string hex = (str + "000000").Substring(1, 6);
                byte[] cb =
                    Enumerable.Range(0, hex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                        .ToArray();
                return new Color32(cb[0], cb[1], cb[2], 255);
            }
            catch
            {
                return Color.white;
            }
        }

        public static void TileToLatLong(double tx, double ty, int zoom, out double lx, out double ly)
        {
            double mapSize = TILE_SIZE << zoom;
            lx = 360 * (Repeat(tx * TILE_SIZE, 0, mapSize - 1) / mapSize - 0.5);
            ly = 90 - 360 * Math.Atan(Math.Exp(-(0.5 - Clamp(ty * TILE_SIZE, 0, mapSize - 1) / mapSize) * 2 * Math.PI)) / Math.PI;
        }

        /// <summary>
        /// Converts tile index to quadkey.
        /// What is the tiles and quadkey, and how it works, you can read here:
        /// http://msdn.microsoft.com/en-us/library/bb259689.aspx
        /// </summary>
        /// <param name="x">Tile X</param>
        /// <param name="y">Tile Y</param>
        /// <param name="zoom">Zoom</param>
        /// <returns>Quadkey</returns>
        public static string TileToQuadKey(int x, int y, int zoom)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = zoom; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((x & mask) != 0) digit++;
                if ((y & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        public static IEnumerable<int> Triangulate(List<Vector2> points)
        {
            List<int> indices = new List<int>();

            int n = points.Count;
            if (n < 3) return indices;

            int[] V = new int[n];
            if (TriangulateArea(points) > 0) for (int v = 0; v < n; v++) V[v] = v;
            else for (int v = 0; v < n; v++) V[v] = (n - 1) - v;

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0) return indices;

                int u = v;
                if (nv <= u) u = 0;
                v = u + 1;
                if (nv <= v) v = 0;
                int w = v + 1;
                if (nv <= w) w = 0;

                if (TriangulateSnip(points, u, v, w, nv, V))
                {
                    int s, t;
                    indices.Add(V[u]);
                    indices.Add(V[v]);
                    indices.Add(V[w]);
                    for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices;
        }

        private static float TriangulateArea(List<Vector2> points)
        {
            int n = points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = points[p];
                Vector2 qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private static bool TriangulateInsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float bp = (C.x - B.x) * (P.y - B.y) - (C.y - B.y) * (P.x - B.x);
            float ap = (B.x - A.x) * (P.y - A.y) - (B.y - A.y) * (P.x - A.x);
            float cp = (A.x - C.x) * (P.y - C.y) - (A.y - C.y) * (P.x - C.x);
            return ((bp >= 0.0f) && (cp >= 0.0f) && (ap >= 0.0f));
        }

        private static bool TriangulateSnip(List<Vector2> points, int u, int v, int w, int n, int[] V)
        {
            Vector2 A = points[V[u]];
            Vector2 B = points[V[v]];
            Vector2 C = points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x)))) return false;
            for (int p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w) continue;
                if (TriangulateInsideTriangle(A, B, C, points[V[p]])) return false;
            }
            return true;
        }
    }
}