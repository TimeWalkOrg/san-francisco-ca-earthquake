/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Linq;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainLookLngLat : MonoBehaviour
    {
        public float distance = 10;
        public float height = 5;
        public float lat;
        public float lng;

        public static bool GetRealWorldPoint(out Vector3 position, double lng, double lat)
        {
            position = new Vector3();
            RealWorldTerrainContainer[] targets = FindObjectsOfType<RealWorldTerrainContainer>();
            RealWorldTerrainContainer target = targets.FirstOrDefault(t => t.Contains(lng, lat));

            if (target == null)
            {
                Debug.Log("Target not found");
                return false;
            }

            return target.GetWorldPosition(lng, lat, out position);
        }

        public static void LookTo(float lng, float lat)
        {
            Vector3 position;
            if (!GetRealWorldPoint(out position, lng, lat)) return;
            Camera.main.transform.LookAt(position);
        }

        public static void MoveTo(float lng, float lat, float distance, float height)
        {
            Vector3 position;
            if (!GetRealWorldPoint(out position, lng, lat)) return;
            Vector3 direction = Camera.main.transform.position - position;
            direction.y = 0;
            Vector3 newPosition = position + direction.normalized * distance;
            newPosition.y += height;
            Camera.main.transform.position = newPosition;
            Camera.main.transform.LookAt(position);
        }
    }
}