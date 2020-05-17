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
using Random = UnityEngine.Random;
#if BUILDR2
using BuildR2;
#endif

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainBuildR2Generator
    {
        public static List<string> alreadyCreated;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static void CreateBuilding(RealWorldTerrainContainer globalContainer, RealWorldTerrainOSMWay way)
        {
#if BUILDR2
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

            int southIndex = -1;
            float southZ = float.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= centerPoint;

                if (points[i].z < southZ)
                {
                    southZ = points[i].z;
                    southIndex = i;
                }
            }

            int prevIndex = southIndex - 1;
            if (prevIndex < 0) prevIndex = points.Count - 1;

            int nextIndex = southIndex + 1;
            if (nextIndex >= points.Count) nextIndex = 0;

            float angle1 = RealWorldTerrainUtils.Angle2D(points[southIndex], points[nextIndex]);
            float angle2 = RealWorldTerrainUtils.Angle2D(points[southIndex], points[prevIndex]);

            if (angle1 < angle2) points.Reverse();

            GameObject house = RealWorldTerrainUtils.CreateGameObject(RealWorldTerrainBuildingGenerator.houseContainer, way.id);
            house.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
            house.AddComponent<RealWorldTerrainBuildRBuilding>();
            Building building = house.AddComponent<Building>();

            building.colliderType = (BuildingColliderTypes)prefs.buildRCollider;
            building.meshType = (BuildingMeshTypes)prefs.buildRRenderMode;
            building.generateInteriors = false;

            IVolume volume = building.AddPlan();

            int numberOfFloors = prefs.buildingFloorLimits.Random();

            if (way.HasTagKey("building:levels"))
            {
                int l;
                if (int.TryParse(way.GetTagValue("building:levels"), out l)) numberOfFloors = l;
            }

            if (numberOfFloors > 1) numberOfFloors -= 1;

            volume.Initialise(points.Select(p => new VolumePoint(new BuildR2.Vector2Int(p.x, p.z))).Reverse().ToList(), numberOfFloors, Random.Range(2.5f, 4f));

            volume.roof.hasDormers = true;
            volume.roof.floorDepth = Random.Range(0, 0.25f);
            volume.roof.minimumDormerSpacing = Random.Range(0.25f, 1f);
            volume.roof.dormerHeight = volume.roof.height * 0.9f;

            if (prefs.buildR2Materials != null && prefs.buildR2Materials.Count > 0)
            {
                RealWorldTerrainBuildR2Material material = prefs.buildR2Materials[Random.Range(0, prefs.buildR2Materials.Count)];

                if (material.roofSurface != null) volume.roof.mainSurface = material.roofSurface;
                volume.roof.type = material.roofType;

                if (material.facades != null && material.facades.Count > 0)
                {
                    int facadeCount = volume.numberOfFacades;
                    for (int f = 0; f < facadeCount; f++) volume.SetFacade(f, material.facades[Random.Range(0, material.facades.Count)]);
                }
            }

            building.MarkModified();

            house.transform.position = centerPoint;

            Visual visual = house.AddComponent<Visual>();
            visual.building = building;
#endif
        }

        public static void Generate(RealWorldTerrainContainer globalContainer)
        {
            if (!RealWorldTerrainBuildingGenerator.loaded)
            {
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

                alreadyCreated = new List<string>();
            }

            EditorUtility.DisplayProgressBar("Generate Buildings", "", 0);

            for (int i = RealWorldTerrainPhase.index; i < RealWorldTerrainBuildingGenerator.ways.Count; i++)
            {
                RealWorldTerrainOSMWay way = RealWorldTerrainBuildingGenerator.ways.Values.ElementAt(i);
                if (alreadyCreated.Contains(way.id)) continue;
                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!String.IsNullOrEmpty(layer) && Int32.Parse(layer) < 0) continue;

                CreateBuilding(globalContainer, way);
                alreadyCreated.Add(way.id);

                float progress = (i + 1) / (float)RealWorldTerrainBuildingGenerator.ways.Count;
                EditorUtility.DisplayProgressBar("Generate Buildings " + (progress * 100).ToString("F2") + "%", "", progress);
            }

            EditorUtility.ClearProgressBar();

            alreadyCreated = null;
            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}