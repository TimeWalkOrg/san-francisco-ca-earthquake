/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.OSM;
using UnityEngine;
using Object = UnityEngine.Object;

#if EASYROADS3D
using EasyRoads3Dv3;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainEasyRoads3DGenerator: RealWorldTerrainRoadGenerator
    {
        public RealWorldTerrainEasyRoads3DGenerator(RealWorldTerrainOSMWay way, RealWorldTerrainContainer container) : base(way, container)
        {

        }

#if EASYROADS3D
        private static ERRoadNetwork roadNetwork;
        private static ERRoadType roadType;
        private static ERRoadType[] erRoadTypes;
        private static string[] roadTypeNames;
        private static Dictionary<string, DelayedConnection> roadsWithConnections;
        private static float roadWidth;

        public override void Create()
        {
            ERRoadType activeRoadType = roadType;
            if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.advanced)
            {
                string highway = way.GetTagValue("highway");
                int index = -1;
                for (int i = 0; i < roadTypeNames.Length; i++)
                {
                    if (highway == roadTypeNames[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1) return;
                if (string.IsNullOrEmpty(prefs.erRoadTypes[index])) return;
                string roadTypeName = prefs.erRoadTypes[index];
                activeRoadType = erRoadTypes.FirstOrDefault(t => t.roadTypeName == roadTypeName);
                if (activeRoadType == null) return;
            }

            if (!string.IsNullOrEmpty(waitFirstConnection) && nodes[waitFirstConnection].usageCount == 2) points[0] = Vector3.Lerp(points[0], points[1], 0.001f);
            if (!string.IsNullOrEmpty(waitLastConnection) && nodes[waitLastConnection].usageCount == 2) points[points.Count - 1] = Vector3.Lerp(points[points.Count - 1], points[points.Count - 2], 0.001f);

            ERRoad road = roadNetwork.CreateRoad("road " + way.id, activeRoadType, points.ToArray());
            if (prefs.erSnapToTerrain) road.SnapToTerrain(true);
            road.gameObject.isStatic = false;
            road.gameObject.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);

            if (!string.IsNullOrEmpty(waitFirstConnection))
            {
                DelayedConnection c;
                if (!roadsWithConnections.TryGetValue(waitFirstConnection, out c))
                {
                    c = new DelayedConnection();
                    roadsWithConnections.Add(waitFirstConnection, c);
                }
                c.road.Add(new DelayedConnectionItem
                {
                    generator = this,
                    road = road,
                    isFront = true
                });
            }
            if (!string.IsNullOrEmpty(waitLastConnection))
            {
                DelayedConnection c;
                if (!roadsWithConnections.TryGetValue(waitLastConnection, out c))
                {
                    c = new DelayedConnection();
                    roadsWithConnections.Add(waitLastConnection, c);
                }
                c.road.Add(new DelayedConnectionItem
                {
                    generator = this,
                    road = road,
                    isFront = false
                });
            }
        }

        public static void CreateConnections(RealWorldTerrainContainer container)
        {
            ERConnection[] connections = roadNetwork.LoadConnections();

            int[] indices = { 0, 1, 2, 3 };

            foreach (var p in roadsWithConnections)
            {
                DelayedConnection dc = p.Value;
                List<DelayedConnectionItem> items = dc.road;
                int count = items.Count;
                if (count < 2) continue;

                RealWorldTerrainOSMNode node = nodes[p.Key];

                Vector3 wp = RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(new Vector3(node.lng, 0, node.lat), container);

                if (count == 2) CreateIConnection(items);
                else if (count == 3) CreateTCross(connections, items, node, wp, indices);
                else if (count == 4) CreateXCross(connections, items, node, wp, indices);
            }
        }

        private static void CreateIConnection(List<DelayedConnectionItem> roads)
        {
            int ri1 = roads[0].isFront ? 0 : roads[0].generator.points.Count - 1;
            int ri2 = roads[1].isFront ? 0 : roads[1].generator.points.Count - 1;
            
            try
            {
                roadNetwork.ConnectRoads(roads[0].road, ri1, roads[1].road, ri2);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message + "\n" + e.StackTrace);
            }
        }

        private static void CreateTCross(ERConnection[] connections, List<DelayedConnectionItem> items, RealWorldTerrainOSMNode node, Vector3 wp, int[] indices)
        {
            ERConnection erc = connections.FirstOrDefault(cc => cc.name == "Default T Crossing");
            if (erc == null) return;

            Vector3[] secondPoints = items.Select(rr => rr.secondPoint).ToArray();

            float ma = 180;
            float angle = 0;
            bool smallAngle = false;

            for (int i = 0; i < 3; i++)
            {
                int j = i + 1;
                if (j >= 3) j -= 3;

                Vector3 p1 = secondPoints[i];
                Vector3 p2 = secondPoints[j];
                float a = RealWorldTerrainUtils.Angle2D(p1, wp, p2, false);
                if (Mathf.Abs(a) < Mathf.Abs(ma))
                {
                    ma = a;
                    angle = RealWorldTerrainUtils.Angle2D(p1, p2);

                    indices[i] = 0;
                    indices[j] = 1;

                    j = i - 1;
                    if (j < 0) j += 3;

                    indices[j] = 3;

                    float ca = RealWorldTerrainUtils.Angle2D(p1, wp, secondPoints[j], false);

                    smallAngle = Mathf.Abs(ca) < 60 || Mathf.Abs(ca) > 120;

                    if (ca < 0)
                    {
                        angle += 180;
                        indices[i] = 1;
                        j += 2;
                        if (j >= 3) j -= 3;
                        indices[j] = 0;
                    }
                }
            }

            if (smallAngle) return;

            erc = roadNetwork.InstantiateConnection(erc, node.id, wp + Vector3.up * 0.1f, new Vector3(0, 90 - angle, 0));

            for (int i = 0; i < 3; i++)
            {
                DelayedConnectionItem r = items[i];

                try
                {
                    int ci = indices[i];
                    if (r.isFront) r.road.ConnectToStart(erc, ci);
                    else r.road.ConnectToEnd(erc, ci);
                }
                catch
                {
                }
            }
        }

        private static void CreateXCross(ERConnection[] connections, List<DelayedConnectionItem> items, RealWorldTerrainOSMNode node, Vector3 wp, int[] indices)
        {
            ERConnection erc = connections.FirstOrDefault(cc => cc.name == "Default X Crossing");
            if (erc == null) return;

            Vector3[] secondPoints = items.Select(rr => rr.secondPoint).ToArray();

            float ma = 180;
            float angle = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    Vector3 p1 = secondPoints[i];
                    Vector3 p2 = secondPoints[j];

                    float a = RealWorldTerrainUtils.Angle2D(p1, wp, p2, false);
                    if (Mathf.Abs(a) < Mathf.Abs(ma))
                    {
                        ma = a;
                        angle = RealWorldTerrainUtils.Angle2D(p1, p2);

                        indices[i] = 0;
                        indices[j] = 1;

                        for (int k = 0; k < 4; k++)
                        {
                            if (k == i || k == j) continue;

                            p2 = secondPoints[k];
                            a = RealWorldTerrainUtils.Angle2D(p1, wp, p2, false);
                            if (a < 0) indices[k] = 2;
                            else indices[k] = 3;
                        }
                    }
                }
            }

            erc = roadNetwork.InstantiateConnection(erc, node.id, wp + Vector3.up * 0.1f, new Vector3(0, 90 - angle, 0));

            for (int i = 0; i < 4; i++)
            {
                DelayedConnectionItem r = items[i];

                try
                {
                    if (r.road == null) Debug.Log("Road is null");

                    int ci = indices[i];
                    if (r.isFront) r.road.ConnectToStart(erc, ci);
                    else r.road.ConnectToEnd(erc, ci);
                }
                catch
                {
                }
            }
        }

        public static void Init()
        {
            if (prefs.roadEngine != "EasyRoads3D") return;

            roadsWithConnections = new Dictionary<string, DelayedConnection>();

            if (Object.FindObjectOfType<ERModularBase>() == null)
            {
                GameObject roadNetworkGO = Object.Instantiate(Resources.Load("ER Road Network")) as GameObject;
                roadNetworkGO.name = "Road Network";
                roadNetworkGO.transform.position = Vector3.zero;
            }

            roadNetwork = new ERRoadNetwork();
            roadNetwork.roadNetwork.importSideObjectsAlert = false;
            roadNetwork.roadNetwork.importRoadPresetsAlert = false;
            roadNetwork.roadNetwork.importCrossingPresetsAlert = false;
            roadNetwork.roadNetwork.importSidewalkPresetsAlert = false;

            ERRoad[] erRoads = roadNetwork.GetRoads();
            for (int i = 0; i < erRoads.Length; i++)
            {
                if (erRoads[i].gameObject.tag != "RWTIgnore") Object.DestroyImmediate(erRoads[i].gameObject);
                else
                {
                    string name = erRoads[i].gameObject.name;
                    if (name.StartsWith("road ")) alreadyCreated.Add(name.Substring(5));
                }
            }

            ERConnection[] erConnections = roadNetwork.GetConnections();
            for (int i = 0; i < erConnections.Length; i++) Object.DestroyImmediate(erConnections[i].gameObject);

            Material roadMaterial = Resources.Load("Materials/roads/single lane") as Material;
            if (roadMaterial == null) roadMaterial = Resources.Load("Materials/roads/road material") as Material;

            roadWidth = 6;
            if (prefs.sizeType == 2)
            {
                double fromX = prefs.leftLongitude;
                double fromY = prefs.topLatitude;
                double toX = prefs.rightLongitude;
                double toY = prefs.bottomLatitude;

                double rangeX = toX - fromX;

                double scfY = Math.Sin(fromY * Mathf.Deg2Rad);
                double sctY = Math.Sin(toY * Mathf.Deg2Rad);
                double ccfY = Math.Cos(fromY * Mathf.Deg2Rad);
                double cctY = Math.Cos(toY * Mathf.Deg2Rad);
                double cX = Math.Cos(rangeX * Mathf.Deg2Rad);
                double sizeX1 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
                double sizeX2 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
                double sizeX = (sizeX1 + sizeX2) / 2.0;
                double sizeZ = RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY);

                RealWorldTerrainVector2i tCount = prefs.terrainCount;
                const float baseScale = 1000;

                double sX = sizeX / tCount.x * baseScale * prefs.terrainScale.x;
                double sZ = sizeZ / tCount.y * baseScale * prefs.terrainScale.z;

                double scaleX = sX / prefs.fixedTerrainSize.x;
                double scaleZ = sZ / prefs.fixedTerrainSize.z;
                roadWidth /= (float)(scaleX + scaleZ) / 2;
            }

            roadType = new ERRoadType
            {
                roadWidth = roadWidth * prefs.erWidthMultiplier,
                roadMaterial = roadMaterial
            };

            if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.advanced)
            {
                erRoadTypes = roadNetwork.roadNetwork.GetRoadTypes();
                roadTypeNames = Enum.GetNames(typeof(RealWorldTerrainRoadType));
            }
        }

        public class DelayedConnection
        {
            public List<DelayedConnectionItem> road = new List<DelayedConnectionItem>();
        }

        public class DelayedConnectionItem
        {
            public ERRoad road;
            public RealWorldTerrainRoadGenerator generator;
            public bool isFront;

            public Vector3 secondPoint
            {
                get
                {
                    if (isFront) return generator.points[1];
                    return generator.points[generator.points.Count - 2];
                }
            }
        }
#else
        public static void Init() {}
#endif
    }
}