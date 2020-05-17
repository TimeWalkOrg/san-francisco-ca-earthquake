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

namespace InfinityCode.RealWorldTerrain.Generators
{
    public static class RealWorldTerrainBuildingPrefabGenerator
    {
        private static List<RealWorldTerrainBuildingPrefab> usedPrefabs;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static void CreateBuilding(RealWorldTerrainContainer globalContainer, RealWorldTerrainOSMWay way)
        {
            List<RealWorldTerrainBuildingPrefab> ps = new List<RealWorldTerrainBuildingPrefab>();

            foreach (RealWorldTerrainBuildingPrefab p in usedPrefabs)
            {
                if (p.tags.Count == 0) continue;

                foreach (RealWorldTerrainBuildingPrefab.OSMTag t in p.tags)
                {
                    if (t.hasEmptyValue)
                    {
                        if (way.HasTagKey(t.key))
                        {
                            ps.Add(p);
                            break;
                        }
                    }
                    else if (t.hasEmptyKey)
                    {
                        if (way.HasTagValue(t.value))
                        {
                            ps.Add(p);
                            break;
                        }
                    }
                    else
                    {
                        if (way.HasTag(t.key, t.value))
                        {
                            ps.Add(p);
                            break;
                        }
                    }
                }
            }

            if (ps.Count == 0)
            {
                foreach (RealWorldTerrainBuildingPrefab p in usedPrefabs)
                {
                    if (p.tags.Count == 0) ps.Add(p);
                }
            }

            if (ps.Count == 0) return;

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

            RealWorldTerrainBuildingPrefab b = ps[Random.Range(0, ps.Count)];

            GameObject instance = GameObject.Instantiate(b.prefab, RealWorldTerrainBuildingGenerator.houseContainer.transform) as GameObject;
            instance.name = way.id;

            Bounds bb = new Bounds(points[0], Vector3.zero);

            float maxSide = -1;
            Vector3 maxPoint1 = Vector3.zero, maxPoint2 = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p1 = points[i];
                int i2 = i + 1;
                if (i2 == points.Count) i2 = 0;
                Vector3 p2 = points[i2];
                float l = (p1 - p2).sqrMagnitude;
                if (maxSide < l)
                {
                    maxSide = l;
                    maxPoint1 = p1;
                    maxPoint2 = p2;
                }

                bb.Encapsulate(p1);
            }

            float a = 90 - RealWorldTerrainUtils.Angle2D(maxPoint1, maxPoint2);

            Collider collider = instance.GetComponent<Collider>();

            if (b.sizeMode == RealWorldTerrainBuildingPrefab.SizeMode.fitToBounds)
            {
                Vector3 p1 = bb.extents;
                Vector3 p2 = new Vector3(p1.x, 0, 0);
                Vector3 p3 = new Vector3(0, 0, p1.z);

                Quaternion r = Quaternion.Euler(0, -a, 0);
                p2 = r * p2;
                p3 = r * p3;

                p1 = new Vector3(
                    Mathf.Max(Mathf.Abs(p2.x), Mathf.Abs(p3.x)), 
                    p1.y, 
                    Mathf.Max(Mathf.Abs(p2.z), Mathf.Abs(p3.z)));

                Bounds ib = collider.bounds;
                Vector3 s = p1;
                Vector3 os = ib.extents;
                s = new Vector3(s.x / os.x, 1, s.z / os.z);

                if (b.heightMode == RealWorldTerrainBuildingPrefab.HeightMode.averageXZ) s.y = (s.x + s.z) / 2;
                else if (b.heightMode == RealWorldTerrainBuildingPrefab.HeightMode.levelBased)
                {
                    float height = 15;
                    string heightStr = way.GetTagValue("height");
                    string levelsStr = way.GetTagValue("building:levels");
                    RealWorldTerrainBuildingGenerator.GetHeightFromString(heightStr, ref height);
                    if (String.IsNullOrEmpty(heightStr) && !String.IsNullOrEmpty(levelsStr))
                    {
                        float h;
                        if (float.TryParse(levelsStr, out h)) height = h * RealWorldTerrainWindow.prefs.buildingFloorHeight;
                    }
                    else height = RealWorldTerrainWindow.prefs.buildingFloorLimits.Random() * RealWorldTerrainWindow.prefs.buildingFloorHeight;

                    s.y = height / os.y * globalContainer.scale.y / 2;
                }
                else if (b.heightMode == RealWorldTerrainBuildingPrefab.HeightMode.fixedHeight)
                {
                    s.y = b.fixedHeight / os.y * globalContainer.scale.y / 2;
                }

                instance.transform.localScale = s;
            }

            instance.transform.position = centerPoint +  new Vector3(0, collider.bounds.extents.y, 0);
            instance.transform.rotation = Quaternion.Euler(0, a, 0);

            instance.AddComponent<RealWorldTerrainOSMMeta>().GetFromOSM(way);
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

                usedPrefabs = new List<RealWorldTerrainBuildingPrefab>(prefs.buildingPrefabs);

                usedPrefabs.RemoveAll(b => b.prefab == null);

                foreach (RealWorldTerrainBuildingPrefab p in usedPrefabs)
                {
                    if (p.tags == null) p.tags = new List<RealWorldTerrainBuildingPrefab.OSMTag>();
                    p.tags.RemoveAll(t => t.isEmpty);
                }

                usedPrefabs.RemoveAll(b => !b.hasBounds);

                if (usedPrefabs.Count == 0)
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

            for (int i = RealWorldTerrainPhase.index; i < RealWorldTerrainBuildingGenerator.ways.Count; i++)
            {
                RealWorldTerrainOSMWay way = RealWorldTerrainBuildingGenerator.ways.Values.ElementAt(i);
                if (way.GetTagValue("building") == "bridge") continue;
                string layer = way.GetTagValue("layer");
                if (!String.IsNullOrEmpty(layer) && Int32.Parse(layer) < 0) continue;

                CreateBuilding(globalContainer, way);

                float progress = (i + 1) / (float)RealWorldTerrainBuildingGenerator.ways.Count;
                EditorUtility.DisplayProgressBar("Instantiate Buildings " + (progress * 100).ToString("F2") + "%", "", progress);
            }

            EditorUtility.ClearProgressBar();

            RealWorldTerrainPhase.phaseComplete = true;
        }
    }
}