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
    public static class RealWorldTerrainMeshGenerator
    {
        private static Mesh mesh;
        private static int lastX;
        private static int lastY;
        private static int lastMeshIndex;
        private static float curDepth;
        private static float nextZ;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static bool GenerateElevation(MeshData data, int hrY, float thfY, double y2, double y1, int hrX, Vector3 s, float thfX, double x1, double x2, double minElevation, double scaledRange, float nodataDepth, int thiX, int thiY, RealWorldTerrainTimer timer)
        {
            double mx1, mx2, my1, my2;
            RealWorldTerrainUtils.LatLongToMercat(x1, y2, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(x2, y1, out mx2, out my2);

            double thx = (mx2 - mx1) / thfX;
            double thy = (my2 - my1) / thfY;

            Vector3[] vertices = data.vertices;
            Vector2[] uv = data.uv;
            int[] triangles = data.triangles;

            for (int hy = lastY; hy < hrY; hy++)
            {
                float ry = hy / thfY;
                double py = hy * thy + my1;

                int iy = hy * hrX;
                float verticleY = hy * s.z;

                for (int hx = 0; hx < hrX; hx++)
                {
                    float rx = hx / thfX;
                    double px = hx * thx + mx1;

                    double elevation = RealWorldTerrainElevationGenerator.GetElevation(px, py);
                    int v = iy + hx;
                    float cy = float.MinValue;
                    if (Math.Abs(elevation - double.MinValue) > double.Epsilon) cy = (float)((elevation - minElevation) / scaledRange);
                    else if (!prefs.generateUnderWater) cy = nodataDepth;
                    else RealWorldTerrainElevationGenerator.hasUnderwater = true;

                    vertices[v] = new Vector3(hx * s.x, cy, verticleY);
                    uv[v] = new Vector2(rx, ry);

                    if (hx < thiX && hy < thiY)
                    {
                        int mv = (hy * thiX + hx) * 6;
                        triangles[mv] = v;
                        triangles[mv + 1] = v + hrX;
                        triangles[mv + 2] = v + 1;
                        triangles[mv + 3] = v + 1;
                        triangles[mv + 4] = v + hrX;
                        triangles[mv + 5] = v + hrX + 1;
                    }
                }

                lastY = hy + 1;
                if (timer.seconds > 1)
                {
                    RealWorldTerrainPhase.phaseProgress = hy / (float)hrY;
                    return false;
                }
            }
            return true;
        }

        private static bool GenerateUnderWater(MeshData data, double minElevation, double elevationRange, Vector3 s, int hrY, int hrX, RealWorldTerrainTimer timer)
        {
            Vector3[] vertices = data.vertices;

            while (RealWorldTerrainElevationGenerator.hasUnderwater)
            {
                bool newHasUnderwater = false;
                bool fillMaxDepth = false;
                double prevDepth = (curDepth - minElevation) / elevationRange;
                curDepth -= RealWorldTerrainElevationGenerator.depthStep;
                if (curDepth <= prefs.nodataValue)
                {
                    curDepth = prefs.nodataValue;
                    fillMaxDepth = true;
                }

                float cDepth = (float)((curDepth - minElevation) / elevationRange * s.y);

                for (int hy = 0; hy < hrY; hy++)
                {
                    bool ignoreTop = false;
                    int cy = hy * hrX;
                    for (int hx = 0; hx < hrX; hx++)
                    {
                        int cx = cy + hx;
                        if (Math.Abs(vertices[cx].y - float.MinValue) < float.Epsilon)
                        {
                            bool ignoreLeft = hx > 0 && vertices[cx - 1].y != prevDepth;
                            if (fillMaxDepth || RealWorldTerrainElevationGenerator.IsSingleDistance(hx, hy, ignoreLeft, ignoreTop))
                            {
                                vertices[cx].y = cDepth;
                                ignoreTop = true;
                            }
                            else
                            {
                                newHasUnderwater = true;
                                ignoreTop = false;
                            }
                        }
                        else ignoreTop = false;
                    }
                }

                RealWorldTerrainElevationGenerator.hasUnderwater = newHasUnderwater;
                if (timer.seconds > 1) return false;
            }
            return true;
        }

        public static void GenerateMesh(RealWorldTerrainItem item)
        {
            int totalVerticles = 0;
            Vector3[] subMeshVerticles = null;
            Vector2[] subMeshUVs = null;
            int[] subMeshTriangles = null;

            int startRow = lastX;
            int index = lastY;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            MeshData data = item["meshdata"] as MeshData;
            Vector3[] vertices = data.vertices;
            Vector2[] uv = data.uv;

            int rows = data.rows;
            int cols = data.cols;
            int rowPerMesh = 65000 / cols;

            while (true)
            {
                int rowCount = startRow + rowPerMesh <= rows ? rowPerMesh : rows - startRow;

                if (totalVerticles != cols * rowCount)
                {
                    totalVerticles = cols * rowCount;
                    subMeshVerticles = new Vector3[totalVerticles];
                    subMeshUVs = new Vector2[totalVerticles];
                    subMeshTriangles = new int[totalVerticles * 6];
                }

                Vector3 position = new Vector3(0, 0, startRow / (float)(rows - 1) * item.size.z);

                for (int y = 0; y < rowCount; y++)
                {
                    int vy = (y + startRow) * cols;
                    int cy = y * cols;

                    for (int x = 0; x < cols; x++)
                    {
                        int vi = vy + x;
                        int sv = cy + x;
                        subMeshVerticles[sv] = vertices[vi] - position;
                        subMeshUVs[sv] = uv[vi];

                        if (x < cols - 1 && y < rowCount - 1)
                        {
                            int mv = sv * 6;
                            subMeshTriangles[mv] = sv;
                            subMeshTriangles[mv + 1] = sv + cols;
                            subMeshTriangles[mv + 2] = sv + 1;
                            subMeshTriangles[mv + 3] = sv + 1;
                            subMeshTriangles[mv + 4] = sv + cols;
                            subMeshTriangles[mv + 5] = sv + cols + 1;
                        }
                    }
                }

                mesh = new Mesh
                {
                    vertices = subMeshVerticles,
                    triangles = subMeshTriangles,
                    uv = subMeshUVs
                };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                string id = item.name + "x" + index;
                string filename = Path.Combine(item.container.folder, id + ".asset");

                AssetDatabase.CreateAsset(mesh, filename);
                AssetDatabase.SaveAssets();

                startRow += rowPerMesh - 1;
                index++;
                if (startRow >= rows) break;

                if (timer.seconds > 1)
                {
                    lastX = startRow;
                    lastY = index;
                    RealWorldTerrainPhase.phaseProgress = startRow / (float)rows;
                    return;
                }
            }

            lastX = 0;
            lastY = 0;
            nextZ = 0;
            data.countMeshes = index;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void GenerateVertices(RealWorldTerrainItem item)
        {
            int hrX = item.prefs.heightmapResolution / item.prefs.textureCount.x;
            int hrY = item.prefs.heightmapResolution / item.prefs.textureCount.y;

            if (item.prefs.heightmapResolution % item.prefs.textureCount.x != 0) hrX++;
            if (item.prefs.heightmapResolution % item.prefs.textureCount.y != 0) hrY++;

            int thiX = hrX - 1;
            int thiY = hrY - 1;

            int verticlesCount = hrX * hrY;

            MeshData data = item["meshdata"] as MeshData;

            if (data == null)
            {
                item["meshdata"] = data = new MeshData
                {
                    vertices = new Vector3[verticlesCount],
                    triangles = new int[thiX * thiY * 6],
                    uv = new Vector2[verticlesCount],
                    rows = hrX,
                    cols = hrY
                };
            }

            double tx = item.leftLongitude;
            double ty = item.topLatitude;
            double bx = item.rightLongitude;
            double by = item.bottomLatitude;
            double minElevation = item.minElevation;
            double elevationRange = item.maxElevation - minElevation;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            float thfX = thiX;
            float thfY = thiY;
            Vector3 s = item.size;
            s.x /= thfX;
            s.z /= thfY;

            double scaledRange = elevationRange / s.y;
            float nodataDepth = (float)((prefs.nodataValue - minElevation) / scaledRange);

            if (!GenerateElevation(data, hrY, thfY, by, ty, hrX, s, thfX, tx, bx, minElevation, scaledRange, nodataDepth, thiX, thiY, timer)) return;
            if (!GenerateUnderWater(data, minElevation, elevationRange, s, hrY, hrX, timer)) return;

            lastX = 0;
            lastY = 0;
            curDepth = 0;
            lastMeshIndex = 0;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void InstantiateMeshes(RealWorldTerrainItem item)
        {
            MeshData data = item["meshdata"] as MeshData;
            int countMeshes = data.countMeshes;
            Material mat = data.material;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));

                string matFilename = Path.Combine(item.container.folder, item.name) + ".mat";
                AssetDatabase.CreateAsset(mat, matFilename);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                data.material = mat = AssetDatabase.LoadAssetAtPath<Material>(matFilename);
            }

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            for (int i = lastMeshIndex; i < countMeshes; i++)
            {
                string id = item.name + "x" + i;
                string filename = Path.Combine(item.container.folder, id + ".asset");
                Vector3 position = new Vector3(0, 0, nextZ);

                GameObject GO = new GameObject(id);
                GO.transform.parent = item.transform;
                GO.transform.localPosition = position;

                mesh = AssetDatabase.LoadAssetAtPath(filename, typeof(Mesh)) as Mesh;
                item.meshFilter = GO.AddComponent<MeshFilter>();
                item.meshFilter.sharedMesh = mesh;
                MeshCollider cl = GO.AddComponent<MeshCollider>();
                cl.sharedMesh = mesh;
                MeshRenderer meshRenderer = GO.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = mat;

                nextZ = cl.bounds.max.z - item.transform.position.z;

                if (timer.seconds > 1)
                {
                    lastMeshIndex = i + 1;
                    RealWorldTerrainPhase.phaseProgress = i / (float)countMeshes;
                    return;
                }
            }

            lastX = 0;
            lastY = 0;
            nextZ = 0;
            lastMeshIndex = 0;
            data.material = null;
            item["meshdata"] = null;

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public class MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uv;
            public int rows;
            public int cols;
            public int countMeshes;
            public Material material;
        }
    }
}