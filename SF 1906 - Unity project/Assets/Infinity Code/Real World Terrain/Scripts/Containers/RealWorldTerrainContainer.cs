/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Linq;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class is added to the resulting container.\n
    /// It contains all information about terrains.
    /// </summary>
    [Serializable]
    [AddComponentMenu("")]
    public class RealWorldTerrainContainer : RealWorldTerrainMonoBase
    {
        /// <summary>
        /// Billboard start.
        /// </summary>
        public float billboardStart = 50;

        /// <summary>
        /// Detail density.
        /// </summary>
        public float detailDensity = 1;

        /// <summary>
        /// Detail distance.
        /// </summary>
        public float detailDistance = 80;

        /// <summary>
        /// The folder in the project where located terrains.
        /// </summary>
        public string folder;

        /// <summary>
        /// Count of terrains.
        /// </summary>
        public RealWorldTerrainVector2i terrainCount;

        /// <summary>
        /// Title
        /// </summary>
        public string title;

        /// <summary>
        /// Tree distance.
        /// </summary>
        public float treeDistance = 2000;

        /// <summary>
        /// The array of elements that belong to the current process of generation.
        /// </summary>
        public RealWorldTerrainItem[] terrains;

        /// <summary>
        /// Gets all instances of RealWorldTerrainContainer.
        /// </summary>
        /// <returns>Instances of RealWorldTerrainContainer</returns>
        public static RealWorldTerrainContainer[] GetInstances()
        {
            return FindObjectsOfType<RealWorldTerrainContainer>().ToArray();
        }

        public override RealWorldTerrainItem GetItemByWorldPosition(Vector3 worldPosition)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                RealWorldTerrainItem item = terrains[i];
                if (item == null) continue;
                Bounds b = new Bounds(item.bounds.center + transform.position, item.bounds.size);
                if (b.min.x <= worldPosition.x && b.min.z <= worldPosition.z && b.max.x >= worldPosition.x && b.max.z >= worldPosition.z)
                {
                    return item;
                }
            }
            return null;
        }

        public override bool GetWorldPosition(double lng, double lat, out Vector3 worldPosition)
        {
            worldPosition = new Vector3();

            if (!Contains(lng, lat))
            {
                Debug.Log("Wrong coordinates");
                return false;
            }

            if (terrains == null || terrains.Length == 0) return false;

            double mx, my;
            RealWorldTerrainUtils.LatLongToMercat(lng, lat, out mx, out my);

            double lX = RealWorldTerrainUtils.Clamp((mx - leftMercator) / (rightMercator - leftMercator), 0, 1);
            double lZ = RealWorldTerrainUtils.Clamp(1 - (my - topMercator) / (bottomMercator - topMercator), 0, 1);

            Bounds cb = new Bounds(bounds.center + transform.position, bounds.size);

            double x = cb.size.x * lX + cb.min.x;
            double z = cb.size.z * lZ + cb.min.z;

            Terrain terrain = null;
            for (int i = 0; i < terrains.Length; i++)
            {
                RealWorldTerrainItem item = terrains[i];
                Bounds b = new Bounds(item.bounds.center + transform.position, item.bounds.size);
                if (b.min.x <= x && b.min.z <= z && b.max.x >= x && b.max.z >= z)
                {
                    terrain = item.terrain;
                    break;
                }
            }

            if (terrain == null) return false;

            double ix = (x - terrain.gameObject.transform.position.x) / terrain.terrainData.size.x;
            double iz = (z - terrain.gameObject.transform.position.z) / terrain.terrainData.size.z;
            double y = terrain.terrainData.GetInterpolatedHeight((float)ix, (float)iz) + terrain.gameObject.transform.position.y;

            worldPosition.x = (float) x;
            worldPosition.y = (float) y;
            worldPosition.z = (float) z;
            return true;
        }

        public override bool GetWorldPosition(Vector2 coordinates, out Vector3 worldPosition)
        {
            return GetWorldPosition(coordinates.x, coordinates.y, out worldPosition);
        }
    }
}