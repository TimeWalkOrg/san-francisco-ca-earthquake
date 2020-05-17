/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.OSM;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

#if PROCEDURAL_TOOLKIT
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainProceduralToolkitBuildingGenerator
    {
        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static void CreateBuilding(RealWorldTerrainContainer globalContainer, RealWorldTerrainOSMWay way)
        {
#if PROCEDURAL_TOOLKIT
            List<Vector3> points = RealWorldTerrainOSMUtils.GetGlobalPointsFromWay(way, RealWorldTerrainBuildingGenerator.nodes);

            if (points.Count < 3) return;

            if (points.First() == points.Last())
            {
                points.Remove(points.Last());
                if (points.Count < 3) return;
            }

            RealWorldTerrainBuildingGenerator.GetGlobalPoints(points, globalContainer);

            for (int i = 0; i < points.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                if ((points[prev] - points[i]).magnitude < 1f)
                {
                    points.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((points[next] - points[i]).magnitude < 1f)
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

            Vector3 centerPoint = Vector3.zero;
            centerPoint = points.Aggregate(centerPoint, (current, point) => current + point) / points.Count;
            centerPoint.y = points.Min(p => p.y);

            float baseHeight = 15;
            float roofHeight = 0;

            List<Vector2> baseVerticles = points.Select(p =>
            {
                Vector3 np = p - centerPoint;
                return new Vector2(np.x, np.z);
            }).ToList();

            var generator = new BuildingGenerator();
            generator.SetFacadePlanningStrategy(prefs.ptFacadePlanningStrategy);
            generator.SetFacadeConstructionStrategy(prefs.ptFacadeConstructionStrategy);
            generator.SetRoofPlanningStrategy(prefs.ptRoofPlanningStrategy);
            generator.SetRoofConstructionStrategy(prefs.ptRoofConstructionStrategy);

            Debug.Log(way.id);

            BuildingGenerator.Config config = new BuildingGenerator.Config();
            config.roofConfig.type = RandomE.GetRandom(RoofType.Flat, RoofType.Hipped, RoofType.Gabled);
            var building = generator.Generate(baseVerticles, config);
            building.position = centerPoint;

            building.name = way.id;
            building.gameObject.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
#endif
        }

        public static void Generate(RealWorldTerrainContainer globalContainer)
        {
            if (!RealWorldTerrainBuildingGenerator.loaded)
            {
                if (prefs.buildingPrefabs == null)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                RealWorldTerrainBuildingGenerator.Load();

                if (RealWorldTerrainBuildingGenerator.ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }

                if (RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
                {
                    RealWorldTerrainItem item = RealWorldTerrainWindow.generateTarget as RealWorldTerrainItem;
                    RealWorldTerrainBuildingGenerator.baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings " + item.x + "x" + (item.container.terrainCount.y - item.y - 1));
                    RealWorldTerrainBuildingGenerator.baseContainer.transform.position = item.transform.position;
                }
                else RealWorldTerrainBuildingGenerator.baseContainer = RealWorldTerrainUtils.CreateGameObject(globalContainer, "Buildings");

                RealWorldTerrainBuildingGenerator.houseContainer = RealWorldTerrainUtils.CreateGameObject(RealWorldTerrainBuildingGenerator.baseContainer, "Houses");
                globalContainer.generatedBuildings = true;

                if (RealWorldTerrainBuildingGenerator.ways.Count == 0)
                {
                    RealWorldTerrainPhase.phaseComplete = true;
                    return;
                }
            }

            EditorUtility.DisplayProgressBar("Generate Buildings", "", 0);

            Debug.Log(RealWorldTerrainBuildingGenerator.ways.Count);

            for (int i = RealWorldTerrainPhase.index; i < RealWorldTerrainBuildingGenerator.ways.Count; i++)
            {
                RealWorldTerrainOSMWay way = RealWorldTerrainBuildingGenerator.ways.Values.ElementAt(i);
                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!String.IsNullOrEmpty(layer) && Int32.Parse(layer) < 0) continue;

                CreateBuilding(globalContainer, way);

                float progress = (i + 1) / (float)RealWorldTerrainBuildingGenerator.ways.Count;
                EditorUtility.DisplayProgressBar("Generate Buildings " + (progress * 100).ToString("F2") + "%", "", progress);
            }

            EditorUtility.ClearProgressBar();

            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}