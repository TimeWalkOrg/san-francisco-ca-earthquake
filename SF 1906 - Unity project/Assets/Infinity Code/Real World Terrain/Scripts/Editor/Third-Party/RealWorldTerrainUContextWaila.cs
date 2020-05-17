/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

#if UCONTEXT

using InfinityCode.uContext.Tools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.ThirdParty
{
    [InitializeOnLoad]
    public static class RealWorldTerrainUContextWaila
    {
        private static RealWorldTerrainContainer container;

        static RealWorldTerrainUContextWaila()
        {
            Waila.OnPrepareTooltip += OnPrepareTooltip;
        }

        private static string OnPrepareTooltip(GameObject go, string str)
        {
            RealWorldTerrainMonoBase item = go.GetComponent<RealWorldTerrainMonoBase>();
            if (item == null) return str;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray.origin, ray.direction, out hit)) return str;
            double lng, lat, alt;
            item.GetCoordinatesByWorldPosition(hit.point, out lng, out lat, out alt);

            return str + "\nLatitude: " + lat + "\nLongitude: " + lng + "\nAltitude: " + alt.ToString("F2") + " m";
        }
    }
}

#endif