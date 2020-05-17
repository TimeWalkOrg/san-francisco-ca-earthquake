/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class contains all the information about the terrain and Real World Terrain settings.
    /// </summary>
    public abstract class RealWorldTerrainMonoBase : MonoBehaviour
    {
        /// <summary>
        /// Coordinates borders of terrain
        /// </summary>
        // public Rect area;

        public Bounds bounds;

        public bool generateGrass;

        public bool generatedRivers;

        public bool generateRoads;

        /// <summary>
        /// Specifies whether to create textures
        /// </summary>
        public bool generateTextures;

        public bool generateTrees;

        /// <summary>
        /// Specifies whether to create buildings
        /// </summary>
        public bool generatedBuildings;

        /// <summary>
        /// Specifies whether to create grass
        /// </summary>
        public bool generatedGrass;

        /// <summary>
        /// Specifies whether to create textures
        /// </summary>
        public bool generatedTextures;

        /// <summary>
        /// Specifies whether to create trees
        /// </summary>
        public bool generatedTrees;

        /// <summary>
        /// Maximal value of elevation
        /// </summary>
        public double maxElevation;

        /// <summary>
        /// Minimal value of elevation
        /// </summary>
        public double minElevation;

        /// <summary>
        /// Reference to the preferences
        /// </summary>
        public RealWorldTerrainPrefsBase prefs;

        /// <summary>
        /// Scale of terrains
        /// </summary>
        public Vector3 scale;

        /// <summary>
        /// Size of terrains in world units
        /// </summary>
        public Vector3 size;

        /// <summary>
        /// Top latitude
        /// </summary>
        public double topLatitude;

        /// <summary>
        /// Top latitude in Mercator projection (0-1)
        /// </summary>
        public double topMercator;

        /// <summary>
        /// Left longitude
        /// </summary>
        public double leftLongitude;

        /// <summary>
        /// Left longitude in Mercator projection (0-1)
        /// </summary>
        public double leftMercator;

        /// <summary>
        /// Bottom latitude
        /// </summary>
        public double bottomLatitude;

        /// <summary>
        /// Bottom latitude in Mercator projection (0-1)
        /// </summary>
        public double bottomMercator;

        /// <summary>
        /// Right longitude
        /// </summary>
        public double rightLongitude;

        /// <summary>
        /// Right longitude in Mercator projection (0-1)
        /// </summary>
        public double rightMercator;

        /// <summary>
        /// Width. Right longitude - left longitude
        /// </summary>
        public double width;

        /// <summary>
        /// Height. Top latitude - bottom latitude 
        /// </summary>
        public double height;

#if BUILDR2
        public List<RealWorldTerrainBuildR2Material> buildR2Facades;
#endif

        private Dictionary<string, object> customFields;
        public double mercatorWidth;
        public double mercatorHeight;

        public object this[string key]
        {
            get
            {
                if (customFields == null) return null;
                object value;
                if (!customFields.TryGetValue(key, out value)) return null;
                return value;
            }
            set
            {
                if (customFields == null) customFields = new Dictionary<string, object>();
                if (value != null) customFields[key] = value;
                else customFields.Remove(key);
            }
        }

        public void ClearCustomFields()
        {
            customFields = null;
        }

        /// <summary>
        /// Checks whether the coordinate in terrain area.
        /// </summary>
        /// <param name="coordinates">Coordinate</param>
        /// <returns>True - coordinate in area, False - otherwise.</returns>
        public bool Contains(Vector2 coordinates)
        {
            return Contains(coordinates.x, coordinates.y);
        }

        /// <summary>
        /// Checks whether the coordinate in terrain area.
        /// </summary>
        /// <param name="lng">Longitude</param>
        /// <param name="lat">Latitude</param>
        /// <returns>True - coordinate in area, False - otherwise.</returns>
        public bool Contains(double lng, double lat)
        {
            return leftLongitude <= lng && rightLongitude >= lng && topLatitude >= lat && bottomLatitude <= lat;
        }

        /// <summary>
        /// Get altitude by location (coordinates).
        /// </summary>
        /// <param name="lng">Longitude</param>
        /// <param name="lat">Latitude</param>
        /// <returns>Altitude (meters)</returns>
        public double GetAltitudeByCoordinates(double lng, double lat)
        {
            if (!Contains(lng, lat)) return 0;

            Vector3 worldPosition;
            GetWorldPosition(lng, lat, out worldPosition);

            Bounds b = new Bounds(bounds.center + transform.position, bounds.size);
            Vector3 offset = worldPosition - b.min;

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                RealWorldTerrainItem currentItem = GetItemByWorldPosition(worldPosition);
                return offset.y / currentItem.terrainData.size.y * (currentItem.maxElevation - currentItem.minElevation);
            }

            return worldPosition.y / b.size.y * (maxElevation - minElevation) + minElevation;
        }

        /// <summary>
        /// Get altitude by location (coordinates).
        /// </summary>
        /// <param name="lng">Longitude</param>
        /// <param name="lat">Latitude</param>
        /// <returns>Altitude (meters)</returns>
        [Obsolete("Use GetAltitudeByCoordinates")]
        public double GetAltitudeByLocation(double lng, double lat)
        {
            return GetAltitudeByCoordinates(lng, lat);
        }

        /// <summary>
        /// Get altitude by Unity World Position.
        /// </summary>
        /// <param name="worldPosition">Unity World Position</param>
        /// <returns>Altitude (meters)</returns>
        public double GetAltitudeByWorldPosition(Vector3 worldPosition)
        {
            Bounds b = new Bounds(bounds.center + transform.position, bounds.size);
            Vector3 offset = worldPosition - b.min;

            if (offset.x < 0 || offset.z < 0) return 0;
            if (offset.x > b.size.x || offset.z > b.size.z) return 0;

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                RealWorldTerrainItem currentItem = GetItemByWorldPosition(worldPosition);
                return offset.y / currentItem.terrainData.size.y * (currentItem.maxElevation - currentItem.minElevation);
            }

            return worldPosition.y / b.size.y * (maxElevation - minElevation) + minElevation;
        }

        /// <summary>
        /// Get coordinates under mouse cursor
        /// </summary>
        /// <param name="coordinates">Geographical coordinates</param>
        /// <param name="cam">Camera</param>
        /// <returns>True - success, False - otherwise</returns>
        public bool GetCoordinatesUnderCursor(out Vector2 coordinates, Camera cam = null)
        {
            return GetCoordinatesByScreenPosition(Input.mousePosition, out coordinates, cam);
        }

        /// <summary>
        /// Converts the screen coordinates into geographic coordinates.
        /// </summary>
        /// <param name="screenPosition">Position in screen space</param>
        /// <param name="coordinates">Geographic coordinates</param>
        /// <param name="cam">Camera</param>
        /// <returns>True - screen coordinates on terrains, False - otherwise</returns>
        public bool GetCoordinatesByScreenPosition(Vector2 screenPosition, out Vector2 coordinates, Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            coordinates = Vector2.zero;

            RaycastHit[] hits = Physics.RaycastAll(cam.ScreenPointToRay(screenPosition));
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider is TerrainCollider || hit.collider is MeshCollider)
                {
                    RealWorldTerrainItem item = hit.transform.GetComponent<RealWorldTerrainItem>();
                    if (item != null) return GetCoordinatesByWorldPosition(hit.point, out coordinates);
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the screen coordinates into geographic coordinates.
        /// </summary>
        /// <param name="screenPosition">Position in screen space</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="altitude">Altitude</param>
        /// <param name="cam">Camera</param>
        /// <returns>True - screen coordinates on terrains, False - otherwise</returns>
        public bool GetCoordinatesByScreenPosition(Vector2 screenPosition, out double longitude, out double latitude, out double altitude, Camera cam = null)
        {
            if (cam == null) cam = Camera.main;
            longitude = latitude = altitude = 0;

            RaycastHit[] hits = Physics.RaycastAll(cam.ScreenPointToRay(screenPosition));
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider is TerrainCollider || hit.collider is MeshCollider)
                {
                    RealWorldTerrainItem item = hit.transform.GetComponent<RealWorldTerrainItem>();
                    if (item != null) return GetCoordinatesByWorldPosition(hit.point, out longitude, out latitude, out altitude);
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the world coordinates into geographic coordinates.
        /// </summary>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <param name="coordinates">Geographic coordinates</param>
        /// <returns>True - world coordinates on terrains, False - otherwise</returns>
        public bool GetCoordinatesByWorldPosition(Vector3 worldPosition, out Vector2 coordinates)
        {
            coordinates = new Vector2();
            double lng, lat;
            bool result = GetCoordinatesByWorldPosition(worldPosition, out lng, out lat);
            coordinates.x = (float)lng;
            coordinates.y = (float)lat;
            return result;
        }

        /// <summary>
        /// Converts the world coordinates into geographic coordinates.
        /// </summary>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <returns>True - world coordinates on terrains, False - otherwise</returns>
        public bool GetCoordinatesByWorldPosition(Vector3 worldPosition, out double longitude, out double latitude)
        {
            Bounds b = new Bounds(bounds.center + transform.position, bounds.size);

            double wrx = (worldPosition.x - b.min.x) / b.size.x;
            double wrz = (b.max.z - worldPosition.z) / b.size.z;

            double px = (rightMercator - leftMercator) * wrx + leftMercator;
            double pz = (bottomMercator - topMercator) * wrz + topMercator;

            RealWorldTerrainUtils.MercatToLatLong(px, pz, out longitude, out latitude);
            return b.Contains(worldPosition);
        }

        /// <summary>
        /// Converts the world coordinates into geographic coordinates.
        /// </summary>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="altitude">Altitude</param>
        /// <returns>True - world coordinates on terrains, False - otherwise</returns>
        public bool GetCoordinatesByWorldPosition(Vector3 worldPosition, out double longitude, out double latitude, out double altitude)
        {
            altitude = 0;

            Bounds b = new Bounds(bounds.center + transform.position, bounds.size);

            double wrx = (worldPosition.x - b.min.x) / b.size.x;
            double wrz = (b.max.z - worldPosition.z) / b.size.z;

            double px1 = (rightMercator - leftMercator) * wrx + leftMercator;
            double pz = (bottomMercator - topMercator) * wrz + topMercator;

            RealWorldTerrainUtils.MercatToLatLong(px1, pz, out longitude, out latitude);

            if (b.min.x > worldPosition.x || b.max.x < worldPosition.x || b.min.z > worldPosition.z || b.max.z < worldPosition.z)
            {
                return false;
            }

            Vector3 offset = worldPosition - b.min;

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                RealWorldTerrainItem currentItem = GetItemByWorldPosition(worldPosition);
                altitude = offset.y / currentItem.terrainData.size.y * (currentItem.maxElevation - currentItem.minElevation) + currentItem.minElevation;
            }
            else
            {
                altitude = worldPosition.y / b.size.y * (maxElevation - minElevation) + minElevation;
            }

            return true;
        }

        /// <summary>
        /// Get RealWorldTerrainItem by Unity World Position.
        /// </summary>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <returns>Instance of RealWorldTerrainItem</returns>
        public abstract RealWorldTerrainItem GetItemByWorldPosition(Vector3 worldPosition);

        /// <summary>
        /// Get Unity World Position by geographic coordinates.
        /// </summary>
        /// <param name="lng">Longitude</param>
        /// <param name="lat">Latitde</param>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <returns>True - success, False - otherwise</returns>
        public abstract bool GetWorldPosition(double lng, double lat, out Vector3 worldPosition);

        /// <summary>
        /// Get Unity World Position by geographic coordinates.
        /// </summary>
        /// <param name="lng">Longitude</param>
        /// <param name="lat">Latitude</param>
        /// <param name="altitude">Altitude</param>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <returns>True - success, False - otherwise</returns>
        public bool GetWorldPosition(double lng, double lat, double altitude, out Vector3 worldPosition)
        {
            bool result = GetWorldPosition(lng, lat, out worldPosition);
            if (result) worldPosition.y = (float)(bounds.size.y * ((altitude - minElevation) / (maxElevation - minElevation)));
            return result;
        }

        /// <summary>
        /// Get Unity World Position by geographic coordinates.
        /// </summary>
        /// <param name="coordinates">Geographic coordinates</param>
        /// <param name="worldPosition">Position in Unity World Space</param>
        /// <returns></returns>
        public abstract bool GetWorldPosition(Vector2 coordinates, out Vector3 worldPosition);

        public void SetCoordinates(double x1, double y1, double x2, double y2, double tlx, double tly, double brx, double bry)
        {
            leftMercator = x1;
            topMercator = y1;
            rightMercator = x2;
            bottomMercator = y2;

            leftLongitude = tlx;
            rightLongitude = brx;
            topLatitude = tly;
            bottomLatitude = bry;

            width = rightLongitude - leftLongitude;
            height = bottomLatitude - topLatitude;

            mercatorWidth = rightMercator - leftMercator;
            mercatorHeight = bottomMercator - topMercator;
        }
    }
}