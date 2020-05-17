/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.IO;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainGrassGenerator
    {
        private static List<int[,]> details;
        private static List<RealWorldTerrainOSMWay> grassWays;
        private static int totalCount;
        public static List<string> alreadyCreated;
        private static float[] detailsInPoint;

        public static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        public static Dictionary<string, RealWorldTerrainOSMWay> ways;
        public static List<RealWorldTerrainOSMRelation> relations;
        public static bool loaded;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static string url
        {
            get
            {
                string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "node({0},{1},{2},{3});way(bn)['landuse'~'grass|forest|meadow|park|pasture|recreation_ground'];(._;>;);out;node({0},{1},{2},{3});way(bn)['natural'~'scrub|wood|heath']; (._;>;);out;node({0},{1},{2},{3});way(bn)['leisure'~'park|golf_course'];(._;>;);out;rel({0},{1},{2},{3})['leisure'~'golf_course'];(._;>;);out;>;out skel qt;rel({0},{1},{2},{3})['landuse'~'forest|park|farmland'];(._;>;);out;>;out skel qt;rel({0},{1},{2},{3})['natural'~'scrub|wood|heath'];(._;>;);out;>;out skel qt;",
                    prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(format);
            }
        }

        public static string filename
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("grass_{0}_{1}_{2}_{3}.osm", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude));
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

            nodes = null;
            ways = null;
            relations = null;

            details = null;
            grassWays = null;
            alreadyCreated = null;
        }

        public static void Download()
        {
            if (!prefs.generateGrass || File.Exists(compressedFilename)) return;
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
            RealWorldTerrainItem[] terrains;
            RealWorldTerrainVector2i terrainCount;
            Vector3 containerPosition;
            if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
            {
                terrains = new[] {RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem};
                terrainCount = RealWorldTerrainVector2i.one;
                containerPosition = terrains[0].transform.position;
            }
            else
            {
                terrains = container.terrains;
                terrainCount = container.terrainCount;
                containerPosition = container.transform.position;
            }

            TerrainData tdata = terrains[0].terrain.terrainData;
            int detailResolution = tdata.detailResolution;

            if (!loaded)
            {
                RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
                loaded = true;

                container.generatedGrass = true;
                alreadyCreated = new List<string>();

                List<DetailPrototype> prototypes = new List<DetailPrototype>();

                foreach (Texture2D grassPrefab in prefs.grassPrefabs)
                {
                    DetailPrototype prototype = new DetailPrototype
                    {
                        prototypeTexture = grassPrefab,
                        renderMode = DetailRenderMode.GrassBillboard
                    };
                    prototypes.Add(prototype);
                }

                details = new List<int[,]>(terrains.Length);
                foreach (RealWorldTerrainItem item in terrains)
                {
                    item.terrain.terrainData.detailPrototypes = prototypes.ToArray();
                    for (int i = 0; i < prefs.grassPrefabs.Count; i++) details.Add(new int[detailResolution, detailResolution]);
                }

                detailsInPoint = new float[prefs.grassPrefabs.Count];

                grassWays = new List<RealWorldTerrainOSMWay>();

                foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
                {
                    RealWorldTerrainOSMWay w = pair.Value;
                    if (w.HasTags("landuse", "grass", "farmland", "forest", "meadow", "park", "pasture", "recreation_ground") ||
                        w.HasTags("leisure", "park", "golf_course") || w.HasTags("natural", "scrub", "wood", "heath")) grassWays.Add(w);
                }

                totalCount = grassWays.Count + terrainCount.x;

                if (grassWays.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }
            }

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();
            float density = prefs.grassDensity / 100f;
            if (density > 1) density = 1;
            density *= 64;

            if (RealWorldTerrainPhase.index < grassWays.Count)
            {
                int tcx = prefs.terrainCount.x;
                int tcy = prefs.terrainCount.y;
                int ctl = terrains.Length;
                int gpc = prefs.grassPrefabs.Count;

                for (int i = RealWorldTerrainPhase.index; i < grassWays.Count; i++)
                {
                    RealWorldTerrainOSMWay way = grassWays[i];

                    if (alreadyCreated.Contains(way.id)) continue;
                    alreadyCreated.Add(way.id);

                    if (way.nodeRefs.Count == 0) continue;

                    float pxmin = float.MaxValue, pxmax = float.MinValue, pymin = float.MaxValue, pymax = float.MinValue;

                    List<Vector3> lPoints = new List<Vector3>();

                    for (int ni = 0; ni < way.nodeRefs.Count; ni++)
                    {
                        string nodeRef = way.nodeRefs[ni];
                        RealWorldTerrainOSMNode node;
                        if (!nodes.TryGetValue(nodeRef, out node)) continue;

                        double mx, my;
                        RealWorldTerrainUtils.LatLongToMercat(node.lng, node.lat, out mx, out my);

                        Vector3 p = RealWorldTerrainEditorUtils.CoordsToWorld(mx, 0, my, container) - containerPosition;
                        p = new Vector3(p.x / tdata.size.x * detailResolution, 0, p.z / tdata.size.z * detailResolution);
                        if (p.x < pxmin) pxmin = p.x;
                        if (p.x > pxmax) pxmax = p.x;
                        if (p.z < pymin) pymin = p.z;
                        if (p.z > pymax) pymax = p.z;
                        lPoints.Add(p);
                    }

                    if (lPoints.Count < 3) continue;

                    Vector3[] points = lPoints.ToArray();

                    for (int x = (int)pxmin; x < pxmax; x++)
                    {
                        int tix = Mathf.FloorToInt(x / (float)detailResolution);
                        if (tix < 0 || tix >= tcx) continue;

                        int tx = x - tix * detailResolution;

                        for (int y = (int)pymin; y < pymax; y++)
                        {
                            int tiy = Mathf.FloorToInt(y / (float)detailResolution);
                            if (tiy >= tcy || tiy < 0) continue;

                            int tIndex = tiy * tcx + tix;
                            if (tIndex < 0 || tIndex >= ctl) continue;

                            bool intersect = RealWorldTerrainUtils.IsPointInPolygon(points, x + 0.5f, y - 0.5f);
                            if (!intersect) continue;

                            int ty = y - tiy * detailResolution;

                            if (gpc == 1) details[tIndex][ty, tx] = (int)density;
                            else
                            {
                                float totalInPoint = 0;
                                int tIndex2 = tIndex * gpc;

                                for (int k = 0; k < gpc; k++)
                                {
                                    float v = Random.Range(0f, 1f);
                                    detailsInPoint[k] = v;
                                    totalInPoint += v;
                                }

                                for (int k = 0; k < gpc; k++)
                                {
                                    int v = (int)(detailsInPoint[k] / totalInPoint * density);
                                    if (v > 255) v = 255;
                                    details[tIndex2 + k][ty, tx] = v;
                                }
                            }
                        }
                    }

                    if (timer.seconds > 1)
                    {
                        RealWorldTerrainPhase.index = i + 1;
                        RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)totalCount;
                        return;
                    }
                }
                RealWorldTerrainPhase.index = grassWays.Count;
            }

            if (RealWorldTerrainPhase.index >= grassWays.Count)
            {
                for (int x = RealWorldTerrainPhase.index - grassWays.Count; x < terrainCount.x; x++)
                {
                    for (int y = 0; y < terrainCount.y; y++)
                    {
                        int tIndex = y * prefs.terrainCount.x + x;
                        for (int k = 0; k < prefs.grassPrefabs.Count; k++)
                        {
                            terrains[tIndex].terrainData.SetDetailLayer(0, 0, k, details[tIndex * prefs.grassPrefabs.Count + k]);
                        }
                    }
                    if (timer.seconds > 1)
                    {
                        RealWorldTerrainPhase.index = grassWays.Count + x + 1;
                        RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)totalCount;
                        return;
                    }
                }
            }

            Dispose();
            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static void OnDownloadComplete(ref byte[] data)
        {
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, compressedFilename);
        }
    }
}
