/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.OSM;
using System.Collections.Generic;
using UnityEngine;

#if ROADARCHITECT
using GSD.Roads;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public class RealWorldTerrainRoadArchitectGenerator: RealWorldTerrainRoadGenerator
    {
        public RealWorldTerrainRoadArchitectGenerator(RealWorldTerrainOSMWay way, RealWorldTerrainContainer container) : base(way, container)
        {
        }

#if ROADARCHITECT
        protected static GSDRoadSystem tRoadSystem;
        protected List<GSDSplineN> splines;

        public override void Create()
        {
            roadGo = tRoadSystem.AddRoad();
            GSDRoad road = roadGo.GetComponent<GSDRoad>();
            road.opt_HeightModEnabled = false;
            road.opt_bShouldersEnabled = type == "primary";
            road.opt_DetailModEnabled = false;
            road.opt_bMaxGradeEnabled = false;
            road.opt_TreeModEnabled = false;

            if (type == "residential")
            {
                road.opt_LaneWidth = 2;
            }

            if (way.HasTagKey("surface"))
            {
                string surface = way.GetTagValue("surface");
                if (surface == "unpaved")
                {
                    road.opt_tRoadMaterialDropdown = GSDRoad.RoadMaterialDropdownEnum.Dirt;
                    road.opt_LaneWidth = 2.5f;
                }
            }

            if (way.HasTagKey("tracktype"))
            {
                road.opt_tRoadMaterialDropdown = GSDRoad.RoadMaterialDropdownEnum.Dirt;
                road.opt_LaneWidth = 2.5f;
            }

            road.transform.position = firstPoint;
            road.gameObject.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);

            Vector3 offset = new Vector3(0, 0.5f, 0);
            splines = new List<GSDSplineN>();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                GameObject tNodeObj = new GameObject("Node" + i);
                GSDSplineN tNode = tNodeObj.AddComponent<GSDSplineN>();
                tNodeObj.AddComponent<RealWorldTerrainRoadArchitectNode>();
                tNodeObj.transform.position = point + offset;
                tNodeObj.transform.parent = road.GSDSplineObj.transform;
                tNode.idOnSpline = i;
                tNode.GSDSpline = road.GSDSpline;
                tNode.bNeverIntersect = true;
                splines.Add(tNode);
            }

            road.UpdateRoad();
        }

        public static void Init()
        {
            if (prefs.roadEngine != "Road Architect") return;

            tRoadSystem = roadContainer.AddComponent<GSDRoadSystem>();
            RealWorldTerrainUtils.CreateGameObject(roadContainer, "Intersections");
        }
#else
        public static void Init() { }
#endif
    }
}