/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainLookLngLat))]
    public class RealWorldTerrainLookLngLatEditor : Editor
    {
        private RealWorldTerrainLookLngLat item;

        private void OnEnable()
        {
            item = (RealWorldTerrainLookLngLat)target;
        }

        public override void OnInspectorGUI()
        {
            item.lat = EditorGUILayout.FloatField("Lat: ", item.lat);
            item.lng = EditorGUILayout.FloatField("lng: ", item.lng);

            if (GUILayout.Button("Look to")) RealWorldTerrainLookLngLat.LookTo(item.lng, item.lat);

            GUILayout.Space(10);
            item.distance = EditorGUILayout.FloatField("Distance: ", item.distance);
            item.height = EditorGUILayout.FloatField("Height: ", item.height);

            if (GUILayout.Button("Move to"))
                RealWorldTerrainLookLngLat.MoveTo(item.lng, item.lat, item.distance, item.height);
        }
    }
}
