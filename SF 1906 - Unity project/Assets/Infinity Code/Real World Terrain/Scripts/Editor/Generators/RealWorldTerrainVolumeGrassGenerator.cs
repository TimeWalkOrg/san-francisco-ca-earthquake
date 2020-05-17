/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

#if VOLUMEGRASS

using System;
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
    public static class RealWorldTerrainVolumeGrassGenerator
    {
        public static bool loaded;

        private static List<RealWorldTerrainOSMWay> grassWays;
        private static int totalCount;
        private static List<string> alreadyCreated;
        private static float[] detailsInPoint;

        public static Dictionary<string, RealWorldTerrainOSMNode> nodes;
        public static Dictionary<string, RealWorldTerrainOSMWay> ways;
        public static List<RealWorldTerrainOSMRelation> relations;
        private static GameObject grassContainer;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static string url
        {
            get
            {
                string format = string.Format(RealWorldTerrainCultureInfo.numberFormat, "node({0},{1},{2},{3});way(bn)['landuse'~'grass|forest|meadow|park|pasture|recreation_ground'];(._;>;);out;node({0},{1},{2},{3});way(bn)['natural'~'scrub|wood']; (._;>;);out;node({0},{1},{2},{3});way(bn)['leisure'~'park|golf_course'];(._;>;);out;node({0},{1},{2},{3});rel(bn)['leisure'~'golf_course']; (._;>;);out;",
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

            grassWays = null;
            alreadyCreated = null;

            grassContainer = null;
        }

        public static void Generate(RealWorldTerrainContainer container)
        {
            if (!loaded)
            {
                RealWorldTerrainOSMUtils.LoadOSM(compressedFilename, out nodes, out ways, out relations);
                loaded = true;

                grassContainer = RealWorldTerrainUtils.CreateGameObject(container, "VolumeGrass");

                container.generatedGrass = true;
                alreadyCreated = new List<string>();

                grassWays = new List<RealWorldTerrainOSMWay>();
                foreach (KeyValuePair<string, RealWorldTerrainOSMWay> pair in ways)
                {
                    RealWorldTerrainOSMWay w = pair.Value;
                    if (w.HasTags("landuse", "grass", "forest", "meadow", "park", "pasture", "recreation_ground") ||
                        w.HasTags("leisure", "park", "golf_course") || w.HasTags("natural", "scrub", "wood"))
                    {
                        grassWays.Add(w);
                    }
                }

                totalCount = grassWays.Count + container.terrainCount.x;

                if (grassWays.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }
            }

            RealWorldTerrainTimer timer = RealWorldTerrainTimer.Start();

            if (RealWorldTerrainPhase.index < grassWays.Count)
            {
                for (int i = RealWorldTerrainPhase.index; i < grassWays.Count; i++)
                {
                    RealWorldTerrainOSMWay way = grassWays[i];

                    if (alreadyCreated.Contains(way.id)) continue;
                    alreadyCreated.Add(way.id);

                    bool hasOutsidePoints = false;

                    List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, nodes);
                    if (prefs.volumeGrassOutsidePoints == RealWorldTerrainVolumeGrassOutsidePoints.removeOutsidePoints)
                    {
                        hasOutsidePoints = points.RemoveAll(p => p.x < container.leftLongitude || p.x > container.rightLongitude || p.z < container.bottomLatitude || p.z > container.topLatitude) > 0;
                    }
                    else
                    {
                        hasOutsidePoints = points.Any(p => p.x < container.leftLongitude || p.x > container.rightLongitude || p.z < container.bottomLatitude || p.z > container.topLatitude);
                    }

                    if (points.Count == 0) continue;
                    int offset = points[0] == points[points.Count - 1] ? 1 : 0;

                    if (points.Count < 3 + offset) continue;

                    GameObject grass = RealWorldTerrainUtils.CreateGameObject(grassContainer, "Grass_" + way.id);
                    VolumeGrass volumeGrass = grass.AddComponent<VolumeGrass>();
                    volumeGrass.max_y_error[0] = 1;
                    volumeGrass.min_edge_length[0] = 5;

                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        Vector3 p = RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(points[j], container);
                        points[j] = p;
                        volumeGrass.AddControlPoint(p, -1);
                    }

                    if (prefs.volumeGrassOutsidePoints == RealWorldTerrainVolumeGrassOutsidePoints.removeOutsidePoints || (!hasOutsidePoints && prefs.volumeGrassOutsidePoints == RealWorldTerrainVolumeGrassOutsidePoints.noMakeMeshesWithOutsidePoints))
                    {
                        volumeGrass.BuildMesh();
                        volumeGrass.state = 1;
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

            Dispose();
            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}

#endif