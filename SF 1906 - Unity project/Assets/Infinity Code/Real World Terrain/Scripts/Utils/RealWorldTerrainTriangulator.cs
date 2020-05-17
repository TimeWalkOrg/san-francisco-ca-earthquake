/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainTriangulator
    {
        private static void AddHole(List<Vector3> input, List<Vector3> hole)
        {
            if (hole == null || hole.Count < 3) return;

            float closestDistance = float.MaxValue;
            int closestIndex1 = -1;
            int closestIndex2 = -1;

            int holeCount = hole.Count;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < holeCount; i++)
            {
                Vector3 p = hole[i];
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
                Vector3 p = hole[i];
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
            input.InsertRange(closestIndex1, hole.Skip(closestIndex2).Take(firstPartSize));
            input.InsertRange(closestIndex1 + firstPartSize, hole.Take(closestIndex2 + 1));
        }

        private static void AddHoles(List<Vector3> input, List<List<Vector3>> holes)
        {
            if (holes == null) return;

            int holeVertices = 0;
            foreach (List<Vector3> hole in holes)
            {
                if (hole == null || hole.Count < 3) continue;
                holeVertices += hole.Count + 1;
            }

            if (input.Capacity < input.Count + holeVertices) input.Capacity = input.Count + holeVertices;

            foreach (List<Vector3> hole in holes) AddHole(input, hole);
        }

        private static int[] GenerateTriangles(Point[] points, bool clockwise)
        {
            int count = points.Length;
            int total = count;
            int[] results = new int[(total - 2) * 3];

            for (int i = 0; i < total; i++) points[i].UpdateWeight();

            Sort(points, 0, count - 1);

            for (int i = 0; i < total; i++) points[i].pindex = i;

            int rindex = 0;
            int start = 0;
            int si = start;

            while (count > 2)
            {
                bool cannotFindPoint = true;
                for (int i = si; i < total; i++)
                {
                    Point v = points[i];
                    if (v.isExternal) continue;
                    if (v.IsExternal(clockwise)) continue;
                    if (count > 4 && v.HasIntersections()) continue;

                    si = i + 1;
                    v.WriteToResult(results, ref rindex);

                    Point next = v.next;
                    Point prev = v.prev;
                    count--;
                    v.Dispose();
                    next.SetPrev(prev);

                    if (count > 3)
                    {
                        next.isExternal = prev.isExternal = false;

                        for (int j = i; j > start; j--)
                        {
                            Point o = points[j - 1];
                            points[j] = o;
                            o.pindex = j;
                        }
                        start++;

                        int nsi = UpdateWeight(points, prev, start, total);
                        if (si > nsi) si = nsi;

                        nsi = UpdateWeight(points, next, start, total);
                        if (si > nsi) si = nsi;
                    }
                    else
                    {
                        next.WriteToResult(results, ref rindex);
                        prev.Dispose();
                        next.Dispose();
                        count--;
                    }
                    cannotFindPoint = false;
                    break;
                }
                if (cannotFindPoint)
                {
                    //Debug.Log("Triangulate Failed");
                    return null;
                }
            }

            return results;
        }

        private static void Sort(Point[] points, int left, int right)
        {
            int i = left, j = right;
            Point pivot = points[(left + right) / 2];
            float weight = pivot.weight;

            while (i <= j)
            {
                while (points[i].weight < weight)
                {
                    i++;
                }

                while (points[j].weight > weight)
                {
                    j--;
                }

                if (i <= j)
                {
                    Point tmp = points[i];
                    points[i] = points[j];
                    points[j] = tmp;

                    i++;
                    j--;
                }
            }

            if (left < j) Sort(points, left, j);
            if (i < right) Sort(points, i, right);
        }

        public static int[] Triangulate(List<Vector2> input, List<List<Vector3>> holes = null, bool clockwise = true)
        {
            if (input == null) return null;
            if (input.Count < 3) return null;

            List<Vector3> input3 = input.Select(i => new Vector3(i.x, 0, i.y)).ToList();
            return Triangulate(input3, holes, clockwise);
        }

        public static int[] Triangulate(List<Vector3> input, List<List<Vector3>> holes = null, bool clockwise = true)
        {
            if (input == null) return null;
            if (input.Count < 3) return null;

            AddHoles(input, holes);
            int count = input.Count;

            if (count == 3) return new[] { 0, 1, 2 };
            if (count == 4) return new[] { 0, 1, 2, 0, 2, 3 };

            Point[] points = new Point[count];
            Point prev = null;

            for (int i = 0; i < count; i++)
            {
                Point current = new Point(i, input[i]);
                current.SetPrev(prev);
                points[i] = current;
                prev = current;
            }

            points[0].SetPrev(prev);

            return GenerateTriangles(points, clockwise);
        }

        private static int UpdateWeight(Point[] points, Point point, int start, int total)
        {
            float oldWeight = point.weight;
            point.UpdateWeight();
            float newWeight = point.weight;

            int index = point.pindex;
            int i = index;

            if (newWeight < oldWeight)
            {
                index--;
                while (index >= start)
                {
                    Point o = points[index];
                    if (o.weight < newWeight)
                    {
                        points[i] = point;
                        break;
                    }

                    points[i] = o;
                    o.pindex = i;
                    i = index;
                    index--;
                }

                if (i == start) points[i] = point;
            }
            else
            {
                index++;
                while (index < total)
                {
                    Point o = points[index];
                    if (o.weight > newWeight)
                    {
                        points[i] = point;
                        break;
                    }

                    points[i] = o;
                    o.pindex = i;
                    i = index;
                    index++;
                }

                if (index == total) points[i] = point;
            }

            point.pindex = i;
            return i;
        }

        private class Point
        {
            public bool isExternal = false;
            public Point next;
            public Point prev;
            public int pindex;
            public float weight;

            private int index;
            private float x;
            private float y;

            public Point(int index, float x, float y)
            {
                weight = 0;
                this.index = index;
                this.x = x;
                this.y = y;
            }

            public Point(int index, Vector3 p) : this(index, p.x, p.z)
            {

            }

            public void Dispose()
            {
                next = null;
                prev = null;
            }

            private bool EqualTo(Point p)
            {
                return Math.Abs(p.x - x) < float.Epsilon && Math.Abs(p.y - y) < float.Epsilon;
            }

            public bool HasIntersections()
            {
                Point p1 = prev;
                Point p2 = next;

                float rx = p2.x - p1.x;
                float ry = p2.y - p1.y;

                Point p3 = p2.next;
                Point p4 = p3.next;

                while (p4 != p1)
                {
                    float d = (p4.y - p3.y) * rx - (p4.x - p3.x) * ry;

                    if (d > 0)
                    {
                        float u_a = (p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x);
                        float u_b = rx * (p1.y - p3.y) - ry * (p1.x - p3.x);

                        if (u_a >= 0 && u_a <= d && u_b >= 0 && u_b <= d)
                        {
                            if (!p1.EqualTo(p3) && !p1.EqualTo(p4) && !p2.EqualTo(p3) && !p2.EqualTo(p4))
                            {
                                return true;
                            }
                        }
                    }
                    p3 = p4;
                    p4 = p4.next;
                }


                return false;
            }

            public bool IsExternal(bool clockwise)
            {
                Point a = prev;
                Point b = next;

                isExternal = ((b.x - a.x) * (y - a.y) - (b.y - a.y) * (x - a.x) >= 0) ^ clockwise;
                return isExternal;
            }

            public void SetPrev(Point other)
            {
                if (other == null) return;

                prev = other;
                other.next = this;
            }

            public override string ToString()
            {
                if (prev == null) return "Point i:" + index + ". Disposed";
                return "Point i:" + index + ", p:" + prev.index + ", n:" + next.index + ", w:" + weight + ", pi:" + pindex;
            }

            public void UpdateWeight()
            {
                float p1x = prev.x;
                float p1y = prev.y;
                float p2x = next.x;
                float p2y = next.y;

                float ax = p1x - x;
                float ay = p1y - y;
                float bx = p2x - x;
                float by = p2y - y;
                float cx = p2x - p1x;
                float cy = p2y - p1y;

                float a = (float)Math.Sqrt(ax * ax + ay * ay);
                float b = (float)Math.Sqrt(bx * bx + by * by);
                float c = (float)Math.Sqrt(cx * cx + cy * cy);
                float p = (a + b + c) / 2;
                weight = p * (p - a) * (p - b) * (p - c);
            }

            public void WriteToResult(int[] results, ref int rindex)
            {
                results[rindex++] = index;
                results[rindex++] = next.index;
                results[rindex++] = prev.index;
            }
        }
    }
}