/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainTreesGenerator
    {
        private static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        private static Dictionary<string, RealWorldTerrainOSMWay> ways;
        private static List<RealWorldTerrainOSMRelation> relations;
        private static bool loaded;

        private static List<RealWorldTerrainOSMNode> treeNodes;
        private static List<RealWorldTerrainOSMWay> woodWays;
        private static List<RealWorldTerrainOSMWay> treeRowWays;
        private static float treeDensity;
        private static int totalTreeCount;
        private static string currentWayID;
        private static HashSet<string> alreadyCreated;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static string url
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("node({0},{1},{2},{3})['natural'~'tree'];(._;>;);out;");
                builder.Append("node({0},{1},{2},{3});(way(bn)['natural'~'wood|tree_row'];way(bn)['landuse'~'forest|park'];);(._;>;);out;");
                builder.Append("(way['landuse'~'forest|park|orchard']({0},{1},{2},{3});relation['landuse'~'forest|park|orchard']({0},{1},{2},{3});relation['leisure'~'nature_reserve']({0},{1},{2},{3});relation['natural'~'wood']({0},{1},{2},{3}););out;>;out skel qt;");
                string _url = string.Format(RealWorldTerrainCultureInfo.numberFormat, builder.ToString(), prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(_url);
            }
        }

        public static string filename
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("trees_{0}_{1}_{2}_{3}.osm", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude));
            }
        }

        public static string compressedFilename
        {
            get
            {
                return filename + "c";
            }
        }

        public static void Dispose()
        {
            loaded = false;

            relations = null;
            ways = null;
            nodes = null;

            treeNodes = null;
            woodWays = null;
            treeRowWays = null;
            alreadyCreated = null;
        }

        public static void Download()
        {
            if (!prefs.generateTrees || File.Exists(compressedFilename)) return;
            if (File.Exists(filename))
            {
                byte[] data = File.ReadAllBytes(filename);
                OnDownloadComplete(ref data);
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

                item.OnData += OnDownloadComplete;
            }
        }

        public static void Generate(RealWorldTerrainContainer container)
        {
            if (prefs.treePrefabs.Count == 0)
            {
                RealWorldTerrainPhase.phaseComplete = true;
                return;
            }

            RealWorldTerrainItem[] terrains;
            Vector3 containerPosition;
            Vector3 containerSize;
            if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
            {
                terrains = new[] { RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem };
                containerPosition = terrains[0].transform.position;
                containerSize = terrains[0].size;
            }
            else
            {
                terrains = container.terrains;
                containerPosition = container.transform.position;
                containerSize = container.size;
            }

            if (!loaded)
            {
                RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
                loaded = true;

                container.generatedTrees = true;

                TreePrototype[] prototypes = new TreePrototype[prefs.treePrefabs.Count];
                for (int i = 0; i < prototypes.Length; i++)
                {
                    prototypes[i] = new TreePrototype
                    {
                        prefab = prefs.treePrefabs[i]
                    };
                }

                foreach (RealWorldTerrainItem item in terrains)
                {
                    item.terrainData.treePrototypes = prototypes;
                    item.terrainData.treeInstances = new TreeInstance[0];
                }

                treeNodes = new List<RealWorldTerrainOSMNode>();
                woodWays = new List<RealWorldTerrainOSMWay>();
                treeRowWays = new List<RealWorldTerrainOSMWay>();

                foreach (KeyValuePair<string, RealWorldTerrainOSMNode> pair in nodes)
                {
                    RealWorldTerrainOSMNode n = pair.Value;
                    if (n.HasTag("natural", "tree")) treeNodes.Add(n);
                }

                foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
                {
                    RealWorldTerrainOSMWay w = pair.Value;
                    if (w.HasTag("natural", "wood") || w.HasTags("landuse", "forest", "park")) woodWays.Add(w);
                    else if (w.HasTag("natural", "tree_row")) treeRowWays.Add(w);
                }

                totalTreeCount = treeNodes.Count + treeRowWays.Count + woodWays.Count;

                if (totalTreeCount == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                alreadyCreated = new HashSet<string>();

                treeDensity = 800f / prefs.treeDensity;
            }

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            double sx, sy, ex, ey;
            RealWorldTerrainUtils.LatLongToMercat(prefs.leftLongitude, prefs.topLatitude, out sx, out sy);
            RealWorldTerrainUtils.LatLongToMercat(prefs.rightLongitude, prefs.bottomLatitude, out ex, out ey);

            if (RealWorldTerrainPhase.index < treeNodes.Count)
            {
                for (int i = RealWorldTerrainPhase.index; i < treeNodes.Count; i++)
                {
                    RealWorldTerrainOSMNode node = treeNodes[i];
                    if (alreadyCreated.Contains(node.id)) continue;
                    alreadyCreated.Add(node.id);

                    double tx, ty;
                    RealWorldTerrainUtils.LatLongToMercat(node.lng, node.lat, out tx, out ty);

                    double rx = (tx - sx) / (ex - sx);
                    double ry = 1 - (ty - sy) / (ey - sy);

                    Vector3 pos = new Vector3((float)(containerPosition.x + containerSize.x * rx), 0, (float)(containerPosition.z + containerSize.z * ry));

                    SetTreeToTerrain(terrains, pos);

                    if (timer.seconds > 1)
                    {
                        RealWorldTerrainPhase.index = i + 1;
                        RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)totalTreeCount;
                        return;
                    }
                }
                RealWorldTerrainPhase.index = treeNodes.Count;
            }

            if (RealWorldTerrainPhase.index < treeRowWays.Count + treeNodes.Count)
            {
                for (int index = RealWorldTerrainPhase.index - treeNodes.Count; index < treeRowWays.Count; index++)
                {
                    RealWorldTerrainOSMWay way = treeRowWays[index];
                    if (alreadyCreated.Contains(way.id)) continue;
                    alreadyCreated.Add(way.id);
                    List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, nodes);

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector3 p = points[i];
                        double tx, ty;
                        RealWorldTerrainUtils.LatLongToMercat(p.x, p.z, out tx, out ty);

                        double rx = (tx - sx) / (ex - sx);
                        double ry = 1 - (ty - sy) / (ey - sy);

                        p.x = (float) (containerPosition.x + containerSize.x * rx);
                        p.z = (float) (containerPosition.z + containerSize.z * ry);

                        points[i] = p;
                    }

                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        int len = Mathf.RoundToInt((points[i] - points[i + 1]).magnitude / treeDensity);
                        if (len > 0)
                        {
                            for (int j = 0; j <= len; j++) SetTreeToTerrain(terrains, Vector3.Lerp(points[i], points[i + 1], j / (float)len));
                        }
                        else SetTreeToTerrain(terrains, points[i]);
                    }

                    if (timer.seconds > 1)
                    {
                        RealWorldTerrainPhase.index = index + treeNodes.Count + 1;
                        RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)totalTreeCount;
                        return;
                    }
                }
                RealWorldTerrainPhase.index = treeNodes.Count + treeRowWays.Count;
            }

            if (RealWorldTerrainPhase.index < treeRowWays.Count + treeNodes.Count + woodWays.Count)
            {
                for (int index = RealWorldTerrainPhase.index - treeRowWays.Count - treeNodes.Count; index < woodWays.Count; index++)
                {
                    RealWorldTerrainOSMWay way = woodWays[index];
                    if (alreadyCreated.Contains(way.id)) continue;
                    alreadyCreated.Add(way.id);
                    List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, nodes);

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector3 p = points[i];
                        double tx, ty;
                        RealWorldTerrainUtils.LatLongToMercat(p.x, p.z, out tx, out ty);

                        double rx = (tx - sx) / (ex - sx);
                        double ry = 1 - (ty - sy) / (ey - sy);

                        p.x = (float)(containerPosition.x + containerSize.x * rx);
                        p.z = (float)(containerPosition.z + containerSize.z * ry);

                        points[i] = p;
                    }

                    Rect rect = RealWorldTerrainUtils.GetRectFromPoints(points);
                    int lx = Mathf.RoundToInt(rect.width / treeDensity);
                    int ly = Mathf.RoundToInt(rect.height / treeDensity);

                    if (lx > 0 && ly > 0)
                    {
                        currentWayID = way.id;
                        GenerateWoodsInArea(container, terrains, lx, ly, rect, points);
                        if (!RealWorldTerrainWindow.isCapturing) return;
                    }

                    if (timer.seconds > 1)
                    {
                        RealWorldTerrainPhase.index = index + treeNodes.Count + treeRowWays.Count + 1;
                        RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)totalTreeCount;
                        return;
                    }
                }

                RealWorldTerrainPhase.index = totalTreeCount;
            }

            Dispose();
            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static void GenerateWoodsInArea(RealWorldTerrainContainer container, RealWorldTerrainItem[] terrains, int lx, int ly, Rect rect, List<Vector3> points)
        {
            RealWorldTerrainMonoBase c = RealWorldTerrainWindow.generateTarget != null ? RealWorldTerrainWindow.generateTarget : container;

            Bounds bounds = c.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            float rVal = 800f / prefs.treeDensity;

            float dx = (rect.xMax - rect.xMin) / lx;
            float dy = (rect.yMax - rect.yMin) / ly;

            int cs = 0;

            Vector3[] ps = points.ToArray();

            int sx = Mathf.Max(Mathf.FloorToInt((min.x - rect.xMin) / dx + 1), 0);
            int ex = Mathf.Min(Mathf.FloorToInt((max.x - rect.xMin) / dx), lx);

            int sy = Mathf.Max(Mathf.FloorToInt((min.z - rect.yMin) / dy + 1), 0);
            int ey = Mathf.Min(Mathf.FloorToInt((max.z - rect.yMin) / dy), ly);

            for (int x = sx; x < ex; x++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Generating trees in the area " + currentWayID, "Trees in the area: " + cs, x / (float) lx))
                {
                    EditorUtility.ClearProgressBar();
                    RealWorldTerrainWindow.CancelCapture();
                    return;
                }

                float rx = x * dx + rect.xMin;

                for (int y = sy; y < ey; y++)
                {
                    float ry = y * dy + rect.yMin;

                    float px = rx + UnityEngine.Random.Range(-rVal, rVal);
                    float pz = ry + UnityEngine.Random.Range(-rVal, rVal);

                    if (RealWorldTerrainUtils.IsPointInPolygon(ps, px, pz))
                    {
                        SetTreeToTerrain(terrains, new Vector3(px, 0, pz));
                        cs++;
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static void OnDownloadComplete(ref byte[] data)
        {
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, compressedFilename);
        }

        private static void SetTreeToTerrain(RealWorldTerrainItem[] terrains, Vector3 pos)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                RealWorldTerrainItem item = terrains[i];
                Terrain terrain = item.terrain;
                TerrainData tData = terrain.terrainData;
                Vector3 terPos = terrain.transform.position;
                Vector3 localPos = pos - terPos;
                float heightmapWidth = (tData.heightmapResolution - 1) * tData.heightmapScale.x;
                float heightmapHeight = (tData.heightmapResolution - 1) * tData.heightmapScale.z;
                if (localPos.x > 0 && localPos.z > 0 && localPos.x < heightmapWidth && localPos.z < heightmapHeight)
                {
                    terrain.AddTreeInstance(new TreeInstance
                    {
                        color = Color.white,
                        heightScale = 1 + UnityEngine.Random.Range(-0.3f, 0.3f),
                        lightmapColor = Color.white,
                        position = new Vector3(localPos.x / heightmapWidth, 0, localPos.z / heightmapHeight),
                        prototypeIndex = UnityEngine.Random.Range(0, tData.treePrototypes.Length),
                        widthScale = 1 + UnityEngine.Random.Range(-0.2f, 0.2f)
                    });
                    break;
                }
            }
        }
    }
}