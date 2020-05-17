/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Generators
{
    public abstract class RealWorldTerrainRoadGenerator
    {
        protected static List<RealWorldTerrainRoadGenerator> roads;
        protected static GameObject roadContainer;

        public bool isDuplicate = false;
        public string type;

        protected RealWorldTerrainContainer container;
        protected string id;
        public List<Vector3> points;
        protected RealWorldTerrainOSMWay way;
        protected List<Vector3> globalPoints;
        protected GameObject roadGo;
        protected static List<string> alreadyCreated;
        protected static bool loaded = false;
        protected static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        protected static Dictionary<string, RealWorldTerrainOSMWay> ways;
        protected static List<RealWorldTerrainOSMRelation> relations;

        protected string waitFirstConnection;
        protected string waitLastConnection;

        public static string url
        {
            get
            {
                string highwayType = "'highway'";
                if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.simple)
                {
                    if ((int)prefs.roadTypes != -1)
                    {
                        BitArray ba = new BitArray(System.BitConverter.GetBytes((int)prefs.roadTypes));
                        List<string> types = new List<string>();
                        for (int i = 0; i < 32; i++)
                        {
                            if (ba.Get(i))
                            {
                                string s = Enum.GetName(typeof(RealWorldTerrainRoadType), (RealWorldTerrainRoadType)(1 << i)).ToLowerInvariant();
                                types.Add(s);
                            }
                        }
                        highwayType += "~'" + string.Join(@"|", types.ToArray()) + "'";
                    }
                }
                
                string data = string.Format(RealWorldTerrainCultureInfo.numberFormat, "node({0},{1},{2},{3});way(bn)[{4}];(._;>;);out;", prefs.bottomLatitude, prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude, highwayType);
                return RealWorldTerrainOSMUtils.osmURL + RealWorldTerrainDownloadManager.EscapeURL(data);
            }
        }

        public static string filename
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.osmCacheFolder, string.Format("roads_{0}_{1}_{2}_{3}_{4}.osm", prefs.bottomLatitude, prefs.leftLongitude,
                    prefs.topLatitude, prefs.rightLongitude, (int)prefs.roadTypes));
            }
        }

        public static string compressedFilename
        {
            get
            {
                return filename + "c";
            }
        }

        protected static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        protected Vector3 secondPoint
        {
            get { return points[1]; }
        }

        protected Vector3 preLastPoint
        {
            get { return points[points.Count - 2]; }
        }

        protected Vector3 lastPoint
        {
            get { return points.Last(); }
        }

        protected Vector3 firstPoint
        {
            get { return points.First(); }
        }

        public RealWorldTerrainRoadGenerator(RealWorldTerrainOSMWay way, RealWorldTerrainContainer container)
        {
            if (roads == null) roads = new List<RealWorldTerrainRoadGenerator>();

            this.container = container;
            this.way = way;
            id = way.id;
            type = this.way.GetTagValue("highway");

            globalPoints = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(this.way, nodes);

            DetectDuplicates();
            if (isDuplicate) return;

            RealWorldTerrainOSMNode firstNode = nodes[way.nodeRefs[0]];
            RealWorldTerrainOSMNode lastNode = nodes[way.nodeRefs.Last()];

            if (firstNode.usageCount > 1) waitFirstConnection = firstNode.id;
            if (lastNode.usageCount > 1) waitLastConnection = lastNode.id;

            points = new List<Vector3>(globalPoints.Count);
            foreach (Vector3 gp in globalPoints) points.Add(RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(gp, container));

            NormalizeDistance();
            TrimPoints();

            roads.Add(this);
        }

        private void DetectDuplicates()
        {
            for (int i = 0; i < roads.Count; i++)
            {
                RealWorldTerrainRoadGenerator r = roads[i];
                if (r.globalPoints.Count != globalPoints.Count) continue;

                bool findDiff = globalPoints.Where((t, j) => (r.globalPoints[j] - t).magnitude > 0.0001f).Any();
                if (!findDiff)
                {
                    isDuplicate = true;
                    return;
                }
            }
        }

        public static void Dispose()
        {
            loaded = false;
            ways = null;
            relations = null;
            nodes = null;
            roadContainer = null;
            alreadyCreated = null;
        }

        public static void Download()
        {
            if (!prefs.generateRoads || prefs.roadTypes == 0 || File.Exists(compressedFilename)) return;
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

        public virtual void Create()
        {

        }

        public static void Generate(RealWorldTerrainContainer container)
        {
            if (!loaded && !Init(container)) return;

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            for (int i = RealWorldTerrainPhase.index; i < roads.Count; i++)
            {
                RealWorldTerrainRoadGenerator road = roads[i];
                if (road.points.Count < 2) continue;
                if (alreadyCreated.Contains(road.id)) continue;
                alreadyCreated.Add(road.id);
                road.Create();

                if (timer.seconds > 1)
                {
                    RealWorldTerrainPhase.index = i + 1;
                    RealWorldTerrainPhase.phaseProgress = RealWorldTerrainPhase.index / (float)roads.Count;
                    return;
                }
            }

#if EASYROADS3D
            if (prefs.erGenerateConnection) RealWorldTerrainEasyRoads3DGenerator.CreateConnections(container);
#endif

            Dispose();
            RealWorldTerrainPhase.phaseComplete = true;
        }

        private static bool Init(RealWorldTerrainContainer container)
        {
            Load();
            if (ways == null || ways.Count == 0)
            {
                RealWorldTerrainPhase.phaseComplete = true;
                return false;
            }

            SplitWays();

            if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
            {
                RealWorldTerrainItem item = RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem;
                roadContainer = RealWorldTerrainUtils.CreateGameObject(container, "Roads " + item.x + "x" + (item.container.terrainCount.y - item.y - 1));
                roadContainer.transform.position = item.transform.position;
            }
            else roadContainer = RealWorldTerrainUtils.CreateGameObject(container, "Roads");

            if (prefs.roadEngine == "EasyRoads3D")
            {
                foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
                {
                    new RealWorldTerrainEasyRoads3DGenerator(pair.Value, container);
                }

                RealWorldTerrainEasyRoads3DGenerator.Init();
            }
            else if (prefs.roadEngine == "Road Architect")
            {
                foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
                {
                    new RealWorldTerrainRoadArchitectGenerator(pair.Value, container);
                }

                RealWorldTerrainRoadArchitectGenerator.Init();
            }

            alreadyCreated = new List<string>();
            return true;
        }

        private static void Load()
        {
            if (prefs.roadTypes == 0) return;
            RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
            loaded = true;
        }

        private void NormalizeDistance()
        {
            int i = 0;
            while (i < points.Count - 1)
            {
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                if ((p1 - p2).magnitude < 10)
                {
                    points[i] = Vector3.Lerp(p1, p2, 0.5f);
                    points.RemoveAt(i + 1);
                }
                else i++;
            }

            i = 0;
            while (i < points.Count - 1)
            {
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                if ((p1 - p2).magnitude > 40) points.Insert(i + 1, Vector3.Lerp(p1, p2, 0.5f));
                else i++;
            }
        }

        private static void OnDownloadComplete(ref byte[] data)
        {
            RealWorldTerrainOSMUtils.GenerateCompressedFile(data, ref nodes, ref ways, ref relations, compressedFilename);
        }

        private static void SplitWays()
        {
            foreach (KeyValuePair<string, RealWorldTerrainOSMNode> node in nodes)
            {
                node.Value.usageCount = 0;
            }

            foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
            {
                RealWorldTerrainOSMWay way = pair.Value;
                foreach (string nid in way.nodeRefs)
                {
                    RealWorldTerrainOSMNode node;
                    if (nodes.TryGetValue(nid, out node)) node.usageCount++;
                }
            }

            List<RealWorldTerrainOSMWay> newWays = new List<RealWorldTerrainOSMWay>();

            foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
            {
                RealWorldTerrainOSMWay way = pair.Value;
                TrySplitWay(way, newWays, way.id, 0);
            }

            foreach (RealWorldTerrainOSMWay w in newWays) ways.Add(w.id, w);
        }

        private void TrimPoints()
        {
            int index = 0;
            Vector3 p1 = container.transform.position;
            Vector3 p2 = p1 + container.size;
            while (index < points.Count)
            {
                Vector3 p = points[index];
                if (p.x < p1.x || p.z < p1.z || p.x > p2.x || p.z > p2.z) points.RemoveAt(index);
                else index++;
            }
        }

        private static void TrySplitWay(RealWorldTerrainOSMWay way, List<RealWorldTerrainOSMWay> newWays, string id, int deep)
        {
            for (int i = 1; i < way.nodeRefs.Count - 1; i++)
            {
                string nid = way.nodeRefs[i];
                RealWorldTerrainOSMNode node;
                if (!nodes.TryGetValue(nid, out node)) continue;
                if (node.usageCount < 2) continue;
                
                RealWorldTerrainOSMWay newWay = new RealWorldTerrainOSMWay();
                newWay.id = id + "_" + deep;
                newWay.tags = new List<RealWorldTerrainOSMTag>(way.tags);
                newWay.nodeRefs = way.nodeRefs.Skip(i).ToList();
                way.nodeRefs = way.nodeRefs.Take(i + 1).ToList();
                newWays.Add(newWay);
                TrySplitWay(newWay, newWays, id, deep + 1);
                break;
            }
        }
    }
}
