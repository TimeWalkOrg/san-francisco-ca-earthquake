/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainBuildingGenerator
    {
        private const int multiRequestZoom = 12;

        public static Func<List<Vector3>, RealWorldTerrainOSMWay, Dictionary<string, RealWorldTerrainOSMNode>, bool> OnGenerateBuilding;

        public static GameObject baseContainer;
        public static GameObject houseContainer;

        private static Material defHouseRoofMaterial;
        private static Material defHouseWallMaterial;

        public static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        public static Dictionary<string, RealWorldTerrainOSMWay> ways;
        public static List<RealWorldTerrainOSMRelation> relations;
        public static bool loaded;

        private static string url
        {
            get
            {
                string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "(way['building']({0},{1},{2},{3});relation['building']({0},{1},{2},{3}););out;>;out skel qt;", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(format);
            }
        }

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static string filename
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude));
            }
        }

        public static string compressedFilename
        {
            get
            {
                return filename + "c";
            }
        }

        private static void AddHole(RealWorldTerrainContainer globalContainer, List<Vector3> input, RealWorldTerrainOSMWay hole)
        {
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(hole, nodes);
            if (points.Count < 3) return;
            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                if ((points[prev] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(next);
                    continue;
                }

                float a1 = RealWorldTerrainUtils.Angle2D(points[prev], points[i]);
                float a2 = RealWorldTerrainUtils.Angle2D(points[i], points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                }
            }

            if (points.Count < 3) return;

            if (!IsReversed(points))
            {
                points.Reverse();
            }

            float closestDistance = float.MaxValue;
            int closestIndex1 = -1;
            int closestIndex2 = -1;

            int holeCount = points.Count;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < holeCount; i++)
            {
                Vector3 p = points[i];
                float px = p.x;
                float pz = p.z;

                if (px < minX) minX = px;
                if (px > maxX) maxX = px;
                if (pz < minZ) minZ = pz;
                if (pz > maxZ) maxZ = pz;
            }

            float cx = (maxX + minX) / 2;
            float cz = (maxZ + minZ) / 2;

            for (int i = 0; i < input.Count; i++)
            {
                Vector3 p = input[i];
                float px = p.x;
                float pz = p.z;
                float distance = (px - cx) * (px - cx) + (pz - cz) * (pz - cz);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex1 = i;
                }
            }

            cx = input[closestIndex1].x;
            cz = input[closestIndex1].z;
            closestDistance = float.MaxValue;

            for (int i = 0; i < holeCount; i++)
            {
                Vector3 p = points[i];
                float px = p.x;
                float pz = p.z;
                float distance = (px - cx) * (px - cx) + (pz - cz) * (pz - cz);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex2 = i;
                }
            }

            int firstPartSize = holeCount - closestIndex2;
            input.Insert(closestIndex1, input[closestIndex1]);
            closestIndex1++;
            input.InsertRange(closestIndex1, points.Skip(closestIndex2).Take(firstPartSize));
            input.InsertRange(closestIndex1 + firstPartSize, points.Take(closestIndex2 + 1));
        }

        private static void AddHouseWallVerticle(List<Vector3> vertices, Vector3 p1, Vector3 p2, float topPoint)
        {
            vertices.Add(p1);
            vertices.Add(new Vector3(p1.x, topPoint, p1.z));
            vertices.Add(p2);
            vertices.Add(new Vector3(p2.x, topPoint, p2.z));
        }

        private static void AnalizeHouseRoofType(RealWorldTerrainOSMWay way, ref float baseHeight,
            ref RealWorldTerrainRoofType roofType, ref float roofHeight)
        {
            string roofShape = way.GetTagValue("roof:shape");
            string roofHeightStr = way.GetTagValue("roof:height");
            string minHeightStr = way.GetTagValue("min_height");
            if (!String.IsNullOrEmpty(roofShape))
            {
                if ((roofShape == "dome" || roofShape == "pyramidal") && !String.IsNullOrEmpty(roofHeightStr))
                {
                    GetHeightFromString(roofHeightStr, ref roofHeight);
                    baseHeight -= roofHeight;
                    roofType = RealWorldTerrainRoofType.dome;
                }
            }
            else if (!String.IsNullOrEmpty(roofHeightStr))
            {
                GetHeightFromString(roofHeightStr, ref roofHeight);
                baseHeight -= roofHeight;
                roofType = RealWorldTerrainRoofType.dome;
            }
            else if (!String.IsNullOrEmpty(minHeightStr))
            {
                float totalHeight = baseHeight;
                GetHeightFromString(minHeightStr, ref baseHeight);
                roofHeight = totalHeight - baseHeight;
                roofType = RealWorldTerrainRoofType.dome;
            }
        }

        private static void AnalizeHouseTags(RealWorldTerrainOSMWay way, ref Material wallMaterial, ref Material roofMaterial, ref float baseHeight, bool useDefaultMaterials)
        {
            string heightStr = way.GetTagValue("height");
            string levelsStr = way.GetTagValue("building:levels");
            GetHeightFromString(heightStr, ref baseHeight);
            if (string.IsNullOrEmpty(heightStr))
            {
                if (!string.IsNullOrEmpty(levelsStr))
                {
                    float h;
                    if (float.TryParse(levelsStr, out h)) baseHeight = h * RealWorldTerrainWindow.prefs.buildingFloorHeight;
                }
                else baseHeight = RealWorldTerrainWindow.prefs.buildingFloorLimits.Random() * RealWorldTerrainWindow.prefs.buildingFloorHeight;
            }

            if (prefs.buildingUseColorTags)
            {
                string colorStr = way.GetTagValue("building:colour");
                if (useDefaultMaterials && !String.IsNullOrEmpty(colorStr)) wallMaterial.color = roofMaterial.color = RealWorldTerrainUtils.StringToColor(colorStr);
            }
        }

        private static void CreateHouse(RealWorldTerrainOSMWay way, RealWorldTerrainContainer globalContainer)
        {
            //if (way.id != "464946662") return;

            float minLng, minLat, maxLng, maxLat;
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, nodes, out minLng, out minLat, out maxLng, out maxLat);
            if (points.Count < 3) return;

            if (maxLng < prefs.leftLongitude ||
                maxLat < prefs.bottomLatitude || 
                minLng > prefs.rightLongitude || 
                minLat > prefs.topLatitude) return;

            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                if ((points[prev] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - points[i]).magnitude < 0.01f)
                {
                    points.RemoveAt(next);
                    continue;
                }

                float a1 = RealWorldTerrainUtils.Angle2D(points[prev], points[i]);
                float a2 = RealWorldTerrainUtils.Angle2D(points[i], points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                }
            }

            if (points.Count < 3) return;

            if (IsReversed(points))
            {
                points.Reverse();
            }

            if (OnGenerateBuilding != null)
            {
                try
                {
                    if (OnGenerateBuilding(points, way, nodes)) return;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception.Message + "\n" + exception.StackTrace);
                    return;
                }
            }

            Vector3 centerPoint = Vector3.zero;
            centerPoint = points.Aggregate(centerPoint, (current, point) => current + point) / points.Count;
            centerPoint.y = points.Min(p => p.y);

            if (way.holes != null)
            {
                for (int i = 0; i < way.holes.Count; i++)
                {
                    RealWorldTerrainOSMWay hole = way.holes[i];
                    if (hole.nodeRefs.First() == hole.nodeRefs.Last()) AddHole(globalContainer, points, hole);
                    else
                    {
                        while (true)
                        {
                            bool success = false;
                            for (int j = i + 1; j < way.holes.Count; j++)
                            {
                                RealWorldTerrainOSMWay h2 = way.holes[j];
                                if (hole.nodeRefs.Last() == h2.nodeRefs.First())
                                {
                                    hole.nodeRefs.AddRange(h2.nodeRefs.Skip(1));
                                    way.holes.RemoveAt(j);
                                    success = true;
                                    break;
                                }
                                if (hole.nodeRefs.Last() == h2.nodeRefs.Last())
                                {
                                    h2.nodeRefs.Reverse();
                                    hole.nodeRefs.AddRange(h2.nodeRefs.Skip(1));
                                    way.holes.RemoveAt(j);
                                    success = true;
                                    break;
                                }
                            }

                            if (!success) break;

                            if (hole.nodeRefs.First() == hole.nodeRefs.Last())
                            {
                                AddHole(globalContainer, points, hole);
                                break;
                            }
                        }
                    }
                }
            }

            bool generateWall = true;

            if (way.HasTagKey("building"))
            {
                string buildingType = way.GetTagValue("building");
                if (buildingType == "roof") generateWall = false;
            }

            float baseHeight = 15;
            float roofHeight = 0;

            Material wallMaterial;
            Material roofMaterial;

            bool useDefaultMaterials = true;

            if (prefs.buildingMaterials == null || prefs.buildingMaterials.Count == 0)
            {
                wallMaterial = GetMaterialByTags("OSM-House-Wall-Material", way.tags, defHouseWallMaterial);
                roofMaterial = GetMaterialByTags("OSM-House-Roof-Material", way.tags, defHouseRoofMaterial);
            }
            else
            {
                useDefaultMaterials = false;
                int rnd = Random.Range(0, prefs.buildingMaterials.Count);
                wallMaterial = prefs.buildingMaterials[rnd].wall != null? prefs.buildingMaterials[rnd].wall: defHouseWallMaterial;
                roofMaterial = prefs.buildingMaterials[rnd].roof != null? prefs.buildingMaterials[rnd].roof: defHouseRoofMaterial;
            }

            wallMaterial = wallMaterial != null ? Object.Instantiate(wallMaterial) as Material : new Material(Shader.Find("Diffuse"));
            roofMaterial = roofMaterial != null ? Object.Instantiate(roofMaterial) as Material : new Material(Shader.Find("Diffuse"));

            string folder = RealWorldTerrainWindow.container.folder + "/Buildings/";
            string materialsFolder = folder + "Materials/";

            if (!Directory.Exists(materialsFolder)) Directory.CreateDirectory(materialsFolder);

            if (prefs.buildingSaveInResult)
            {
                string wallMaterialPath = materialsFolder + "House " + way.id + " Wall.mat";
                string roofMaterialPath = materialsFolder + "House " + way.id + " Roof.mat";

                AssetDatabase.CreateAsset(wallMaterial, wallMaterialPath);
                AssetDatabase.CreateAsset(roofMaterial, roofMaterialPath);

                wallMaterial = AssetDatabase.LoadAssetAtPath<Material>(wallMaterialPath);
                roofMaterial = AssetDatabase.LoadAssetAtPath<Material>(roofMaterialPath);
            }

            RealWorldTerrainRoofType roofType = RealWorldTerrainRoofType.flat;
            AnalizeHouseTags(way, ref wallMaterial, ref roofMaterial, ref baseHeight, useDefaultMaterials);
            AnalizeHouseRoofType(way, ref baseHeight, ref roofType, ref roofHeight);

            Vector3[] baseVerticles = points.Select(p => p - centerPoint).ToArray();

            GameObject houseGO = RealWorldTerrainUtils.CreateGameObject(houseContainer, "House " + way.id);
            houseGO.transform.position = centerPoint;

            RealWorldTerrainBuilding house = houseGO.AddComponent<RealWorldTerrainBuilding>();
            house.baseHeight = baseHeight;
            house.baseVerticles = baseVerticles;
            house.container = RealWorldTerrainWindow.container;
            house.roofHeight = roofHeight;
            house.roofType = roofType;

            house.wall = generateWall ? CreateHouseWall(houseGO, baseVerticles, globalContainer.scale, baseHeight, wallMaterial, way.id) : null;
            house.roof = CreateHouseRoof(houseGO, baseVerticles, globalContainer.scale, baseHeight, roofHeight, roofType, roofMaterial, way.id);
            if (house.roof.sharedMesh == null)
            {
                Debug.Log("Wrong roof: " + way.id + ", points: " + points.Count);
            } 

            string meshesPath = folder + "Meshes/";

            if (!Directory.Exists(meshesPath)) Directory.CreateDirectory(meshesPath);

            if (generateWall && prefs.buildingSaveInResult)
            {
                string wallMeshPath = meshesPath + "House " + way.id + " Wall.asset";
                AssetDatabase.CreateAsset(house.wall.sharedMesh, wallMeshPath);
                house.wall.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(wallMeshPath);
            }

            if (house.roof.sharedMesh != null && prefs.buildingSaveInResult)
            {
                string roofMeshPath = meshesPath + "House " + way.id + " Roof.asset";
                AssetDatabase.CreateAsset(house.roof.sharedMesh, roofMeshPath);
                house.roof.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(roofMeshPath);
            }

            houseGO.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
        }

        private static MeshFilter CreateHouseRoof(GameObject houseGO, Vector3[] baseVerticles, Vector3 scale,
            float baseHeight, float roofHeight, RealWorldTerrainRoofType roofType, Material material, string id)
        {
            GameObject wallGO = new GameObject("Roof");
            wallGO.transform.parent = houseGO.transform;
            wallGO.transform.localPosition = Vector3.zero;

            return RealWorldTerrainOSMUtils.AppendMesh(wallGO,
                CreateHouseRoofMesh(baseVerticles, scale, baseHeight, roofHeight, roofType, houseGO.name), material,
                string.Format("House_{0}_Roof", id));
        }

        private static void CreateHouseRoofDome(Vector3 scale, float height, List<Vector3> vertices, List<int> triangles)
        {
            Vector3 roofTopPoint = Vector3.zero;
            roofTopPoint = vertices.Aggregate(roofTopPoint, (current, point) => current + point) / vertices.Count;
            roofTopPoint.y = height * scale.y;
            int vIndex = vertices.Count;

            for (int i = 0; i < vertices.Count; i++)
            {
                int p1 = i;
                int p2 = i + 1;
                if (p2 >= vertices.Count) p2 -= vertices.Count;

                triangles.AddRange(new[] { p1, p2, vIndex });
            }

            vertices.Add(roofTopPoint);
        }

        public static Mesh CreateHouseRoofMesh(Vector3[] baseVerticles, Vector3 scale, float baseHeight, float roofHeight,
            RealWorldTerrainRoofType roofType, string name, bool inverted = false)
        {
            List<Vector2> roofPoints = new List<Vector2>();
            List<Vector3> vertices = new List<Vector3>();

            CreateHouseRoofVerticles(baseVerticles, vertices, roofPoints, scale, baseHeight);
            int[] triangles =
                CreateHouseRoofTriangles(scale, vertices, roofType, roofPoints, baseHeight, roofHeight).ToArray();

            if (inverted) triangles = triangles.Reverse().ToArray();

            float minX = vertices.Min(p => p.x);
            float minZ = vertices.Min(p => p.z);
            float maxX = vertices.Max(p => p.x);
            float maxZ = vertices.Max(p => p.z);
            float offX = maxX - minX;
            float offZ = maxZ - minZ;

            Vector2[] uvs = vertices.Select(v => new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ)).ToArray();

            Mesh mesh = new Mesh
            {
                name = name + " Roof",
                vertices = vertices.ToArray(),
                uv = uvs,
                triangles = triangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static List<int> CreateHouseRoofTriangles(Vector3 scale, List<Vector3> vertices,
            RealWorldTerrainRoofType roofType, List<Vector2> roofPoints, float baseHeight, float roofHeight)
        {
            List<int> triangles = new List<int>();
            if (roofType == RealWorldTerrainRoofType.flat)
            {
                int[] trs = RealWorldTerrainTriangulator.Triangulate(roofPoints);
                if (trs != null) triangles.AddRange(trs);
            }
            else if (roofType == RealWorldTerrainRoofType.dome)
            {
                CreateHouseRoofDome(scale, baseHeight + roofHeight, vertices, triangles);
            }
            return triangles;
        }

        private static void CreateHouseRoofVerticles(Vector3[] baseVerticles, List<Vector3> verticles,
            List<Vector2> roofPoints, Vector3 scale, float baseHeight)
        {
            float topPoint = baseVerticles.Max(v => v.y) + baseHeight * scale.y;
            foreach (Vector3 p in baseVerticles)
            {
                Vector3 tv = new Vector3(p.x, topPoint, p.z);
                Vector2 rp = new Vector2(p.x, p.z);

                verticles.Add(tv);
                roofPoints.Add(rp);
            }
        }

        private static MeshFilter CreateHouseWall(GameObject houseGO, Vector3[] baseVerticles, Vector3 scale,
            float baseHeight, Material material, string id)
        {
            GameObject wallGO = new GameObject("Wall");
            wallGO.transform.parent = houseGO.transform;
            wallGO.transform.localPosition = Vector3.zero;

            return RealWorldTerrainOSMUtils.AppendMesh(wallGO, CreateHouseWallMesh(baseVerticles, scale, baseHeight, houseGO.name), material, string.Format("House_{0}_Wall", id));
        }

        public static Mesh CreateHouseWallMesh(Vector3[] baseVerticles, Vector3 scale, float baseHeight, string name, bool inverted = false)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            bool reversed = CreateHouseWallVerticles(scale, baseHeight, baseVerticles, vertices, uvs);
            if (inverted) reversed = !reversed;
            int[] triangles = CreateHouseWallTriangles(vertices, reversed).ToArray();

            Mesh mesh = new Mesh
            {
                name = name + " Wall",
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static List<int> CreateHouseWallTriangles(List<Vector3> vertices, bool reversed)
        {
            List<int> triangles = new List<int>();
            for (int i = 0; i < vertices.Count / 4; i++) triangles.AddRange(GetHouseWallTriangle(vertices.Count, reversed, i));
            return triangles;
        }

        private static bool CreateHouseWallVerticles(Vector3 scale, float baseHeight, Vector3[] baseVerticles, List<Vector3> vertices, List<Vector2> uvs)
        {
            float topPoint = baseVerticles.Max(v => v.y) + baseHeight * scale.y;

            for (int i = 0; i < baseVerticles.Length; i++)
            {
                Vector3 p1 = baseVerticles[i];
                Vector3 p2 = i < baseVerticles.Length - 1? baseVerticles[i + 1]: baseVerticles[0];
                AddHouseWallVerticle(vertices, p1, p2, topPoint);
            }

            float totalDistance = 0;
            float bottomPoint = float.MaxValue;

            for (int i = 0; i < vertices.Count / 4; i++)
            {
                int i1 = Mathf.RoundToInt(Mathf.Repeat(i * 4, vertices.Count));
                int i2 = Mathf.RoundToInt(Mathf.Repeat((i + 1) * 4, vertices.Count));
                totalDistance += (vertices[i1] - vertices[i2]).magnitude;
                if (bottomPoint > vertices[i * 4].y) bottomPoint = vertices[i * 4].y;
            }

            totalDistance += (vertices[vertices.Count - 4] - vertices[0]).magnitude;

            float currentDistance = 0;
            float yRange = topPoint - bottomPoint;
            float nextU = 0;

            for (int i = 0; i < vertices.Count / 4; i++)
            {
                int i1 = Mathf.RoundToInt(Mathf.Repeat(i * 4, vertices.Count));
                int i2 = Mathf.RoundToInt(Mathf.Repeat((i + 1) * 4, vertices.Count));
                float curU = nextU;
                uvs.Add(new Vector2(curU, (vertices[i * 4].y - bottomPoint) / yRange));
                uvs.Add(new Vector2(curU, 1));

                currentDistance += (vertices[i1] - vertices[i2]).magnitude;
                nextU = currentDistance / totalDistance;

                uvs.Add(new Vector2(nextU, (vertices[i * 4 + 2].y - bottomPoint) / yRange));
                uvs.Add(new Vector2(nextU, 1));
            }

            int southIndex = -1;
            float southZ = float.MaxValue;

            for (int i = 0; i < baseVerticles.Length; i++)
            {
                if (baseVerticles[i].z < southZ)
                {
                    southZ = baseVerticles[i].z;
                    southIndex = i;
                }
            }

            int prevIndex = southIndex - 1;
            if (prevIndex < 0) prevIndex = baseVerticles.Length - 1;

            int nextIndex = southIndex + 1;
            if (nextIndex >= baseVerticles.Length) nextIndex = 0;

            float angle1 = RealWorldTerrainUtils.Angle2D(baseVerticles[southIndex], baseVerticles[nextIndex]);
            float angle2 = RealWorldTerrainUtils.Angle2D(baseVerticles[southIndex], baseVerticles[prevIndex]);

            return angle1 < angle2;
        }

        public static void Dispose()
        {
            loaded = false;

            ways = null;
            nodes = null;
            relations = null;

            defHouseRoofMaterial = null;
            defHouseWallMaterial = null;

            baseContainer = null;
            houseContainer = null;
        }

        public static void Download()
        {
            if (!prefs.generateBuildings) return;
            if (prefs.buildingSingleRequest) DownloadSingleRequest();
            else DownloadMultiRequests();
        }

        private static void DownloadMultiRequests()
        {
            double tlx, tly, brx, bry;

            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, multiRequestZoom, out tlx, out tly);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, multiRequestZoom, out brx, out bry);

            int itlx = (int) tlx;
            int itly = (int) tly;
            int ibrx = (int) brx + 1;
            int ibry = (int) bry + 1;

            int maxX = 1 << multiRequestZoom;
            if (itlx > ibrx) ibrx += maxX;

            for (int x = itlx; x < ibrx; x++)
            {
                int cx = x;
                if (cx >= maxX) cx -= maxX;

                for (int y = itly; y < ibry; y++)
                {
                    RealWorldTerrainUtils.TileToLatLong(cx, y, multiRequestZoom, out tlx, out tly);
                    RealWorldTerrainUtils.TileToLatLong(cx + 1, y + 1, multiRequestZoom, out brx, out bry);

                    string fn = Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", bry, tlx, tly, brx));
                    string cfn = fn + "c";
                    if (File.Exists(cfn)) continue;
                    if (File.Exists(fn))
                    {
                        byte[] data = File.ReadAllBytes(fn);
                        OnDownloadComplete(ref data, cfn);
                    }
                    else
                    {
                        string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "(way['building']({0},{1},{2},{3});relation['building']({0},{1},{2},{3}););out;>;out skel qt;", bry, tlx, tly, brx);
                        string partURL = RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(format);
                        
                        RealWorldTerrainDownloadItemUnityWebRequest item = new RealWorldTerrainDownloadItemUnityWebRequest(partURL)
                        {
                            filename = fn,
                            averageSize = 600000,
                            exclusiveLock = RealWorldTerrainOSMUtils.OSMLocker,
                            ignoreRequestProgress = true
                        };

                        item.OnData += delegate(ref byte[] data)
                        {
                            OnDownloadComplete(ref data, cfn);
                        };
                    }
                }
            }
        }

        private static void DownloadSingleRequest()
        {
            if (File.Exists(compressedFilename)) return;
            if (File.Exists(filename))
            {
                byte[] data = File.ReadAllBytes(filename);
                OnDownloadComplete(ref data, compressedFilename);
            }
            else
            {
                RealWorldTerrainDownloadItemUnityWebRequest item = new RealWorldTerrainDownloadItemUnityWebRequest(url)
                {
                    filename = filename,
                    averageSize = 600000,
                    exclusiveLock = RealWorldTerrainOSMUtils.OSMLocker,
                    ignoreRequestProgress = true
                };

                item.OnData += delegate(ref byte[] data)
                {
                    OnDownloadComplete(ref data, compressedFilename);
                };
            }
        }

        public static string FixPathString(string path)
        {
            return RealWorldTerrainUtils.ReplaceString(path, new[] { ":", "/", "\\", "=" }, "-");
        }

        public static void Generate(RealWorldTerrainContainer globalContainer)
        {
            if (!loaded)
            {
                Load();

                if (ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
                {
                    RealWorldTerrainItem item = RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem;
                    baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings " + item.x + "x" + (item.container.terrainCount.y - item.y - 1));
                    baseContainer.transform.position = item.transform.position;
                }
                else baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings");
                houseContainer = RealWorldTerrainUtils.CreateGameObject(baseContainer, "Houses");

                defHouseWallMaterial = RealWorldTerrainEditorUtils.FindMaterial("Default-House-Wall-Material.mat");
                defHouseRoofMaterial = RealWorldTerrainEditorUtils.FindMaterial("Default-House-Roof-Material.mat");
                globalContainer.generatedBuildings = true;
            }

            GenerateHouses(globalContainer);

            if (!RealWorldTerrainPhase.phaseComplete) RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)(ways.Count);
            else RealWorldTerrainPhase.phaseProgress = 1;
        }

        private static void GenerateHouses(RealWorldTerrainContainer globalContainer)
        {
            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            while (RealWorldTerrainPhase.index < ways.Count)
            {
                if (timer.seconds > 1) return;

                RealWorldTerrainOSMWay way = ways.Values.ElementAt(RealWorldTerrainPhase.index);
                RealWorldTerrainPhase.index++;

                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!string.IsNullOrEmpty(layer))
                {
                    int l;
                    if (int.TryParse(layer, out l) && l < 0) continue;
                }

                CreateHouse(way, globalContainer);
            }

            RealWorldTerrainPhase.phaseComplete = true;
        }

        public static void GetGlobalPoints(List<Vector3> points, RealWorldTerrainContainer globalContainer)
        {
            bool hasNoValuePoints = false;
            bool[] noValues = new bool[points.Count];
            Vector3 pos = globalContainer.transform.position;
            for (int i = 0; i < points.Count; i++)
            {
                bool success;
                Vector3 p = RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(points[i], globalContainer, Vector3.zero, out success);
                noValues[i] = !success;
                if (!success) hasNoValuePoints = true;
                else p += pos;

                points[i] = p;
            }

            if (!hasNoValuePoints) return;

            float sy = 0;
            int cy = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (!noValues[i])
                {
                    sy += points[i].y;
                    cy++;
                }
            }

            if (cy > 0) sy /= cy;
            else sy = RealWorldTerrainWindow.prefs.nodataValue * globalContainer.scale.y;

            for (int i = 0; i < points.Count; i++)
            {
                if (noValues[i])
                {
                    Vector3 p = points[i];
                    p.y = sy;
                    points[i] = p;
                }
            }
        }

        public static void GetHeightFromString(string str, ref float height)
        {
            if (String.IsNullOrEmpty(str)) return;
            if (float.TryParse(str, out height)) return;

            if (str.Substring(str.Length - 2, 2) == "cm")
            {
                float.TryParse(str.Substring(0, str.Length - 2), out height);
                height /= 10;
            }
            else if (str.Substring(str.Length - 1, 1) == "m") float.TryParse(str.Substring(0, str.Length - 1), out height);
        }

        private static int[] GetHouseWallTriangle(int countVertices, bool reversed, int i)
        {
            int p1 = i * 4;
            int p2 = i * 4 + 2;
            int p3 = i * 4 + 3;
            int p4 = i * 4 + 1;

            if (p2 >= countVertices) p2 -= countVertices;
            if (p3 >= countVertices) p3 -= countVertices;

            if (reversed) return new[] { p1, p4, p3, p1, p3, p2 };
            return new[] { p2, p3, p1, p3, p4, p1 };
        }

        public static Material GetMaterialByTags(string materialName, List<RealWorldTerrainOSMTag> tags, Material defaultMaterial)
        {
            if (RealWorldTerrainOSMUtils.projectMaterials == null)
                RealWorldTerrainOSMUtils.projectMaterials = Directory.GetFiles("Assets", "*.mat", SearchOption.AllDirectories);
            foreach (RealWorldTerrainOSMTag tag in tags)
            {
                string matName = string.Format("{0}({1}={2})", materialName, FixPathString(tag.key), FixPathString(tag.value));
                foreach (string projectMaterial in RealWorldTerrainOSMUtils.projectMaterials)
                {
                    if (projectMaterial.Contains(matName))
                    {
                        string assetFN = projectMaterial.Replace("\\", "/");
                        return new Material((Material)AssetDatabase.LoadAssetAtPath(assetFN, typeof(Material)));
                    }
                }
            }

            return new Material(defaultMaterial) { color = Color.white }; ;
        }

        private static bool IsReversed(List<Vector3> points)
        {
            float minZ = float.MaxValue;
            int i2 = -1;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p = points[i];
                if (p.z < minZ)
                {
                    minZ = p.z;
                    i2 = i;
                }
            }

            int i1 = i2 - 1;
            int i3 = i2 + 1;
            if (i1 < 0) i1 += points.Count;
            if (i3 >= points.Count) i3 -= points.Count;

            Vector3 p1 = points[i1];
            Vector3 p2 = points[i2];
            Vector3 p3 = points[i3];

            Vector3 s1 = p2 - p1;
            Vector3 s2 = p3 - p1;

            Vector3 side1 = s1;
            Vector3 side2 = s2;
            Vector3 perp = Vector3.Cross(side1, side2);

            bool reversed = perp.y <= 0;
            return reversed;
        }

        public static void Load()
        {
            if (prefs.buildingSingleRequest) LoadSinglePart();
            else LoadMultiParts();
            loaded = true;
        }

        private static void LoadSinglePart()
        {
            RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
        }

        private static void LoadMultiParts()
        {
            double tlx, tly, brx, bry;

            RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, multiRequestZoom, out tlx, out tly);
            RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, multiRequestZoom, out brx, out bry);

            int itlx = (int)tlx;
            int itly = (int)tly;
            int ibrx = (int)brx + 1;
            int ibry = (int)bry + 1;

            int maxX = 1 << multiRequestZoom;
            if (itlx > ibrx) ibrx += maxX;

            nodes = new Dictionary<string, RealWorldTerrainOSMNode>();
            ways = new Dictionary<string, RealWorldTerrainOSMWay>();
            relations = new List<RealWorldTerrainOSMRelation>();

            for (int x = itlx; x < ibrx; x++)
            {
                int cx = x;
                if (cx >= maxX) cx -= maxX;

                for (int y = itly; y < ibry; y++)
                {
                    RealWorldTerrainUtils.TileToLatLong(cx, y, multiRequestZoom, out tlx, out tly);
                    RealWorldTerrainUtils.TileToLatLong(cx + 1, y + 1, multiRequestZoom, out brx, out bry);

                    string fn = Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("buildings_{0}_{1}_{2}_{3}.osm", bry, tlx, tly, brx));
                    string cfn = fn + "c";

                    Dictionary<string, RealWorldTerrainOSMNode> ns;
                    Dictionary<string, RealWorldTerrainOSMWay> ws;
                    List<RealWorldTerrainOSMRelation> rs;
                    RealWorldTerrainOSMUtils.LoadOSM(cfn, out ns, out ws, out rs);

                    foreach (KeyValuePair<string, RealWorldTerrainOSMNode> pair in ns)
                    {
                        if (!nodes.ContainsKey(pair.Key)) nodes.Add(pair.Key, pair.Value);
                    }

                    foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ws)
                    {
                        if (!ways.ContainsKey(pair.Key)) ways.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        private static void OnDownloadComplete(ref byte[] data, string fn)
        {
            nodes = null;
            ways = null;
            relations = null;
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, fn);
        }
    }
}