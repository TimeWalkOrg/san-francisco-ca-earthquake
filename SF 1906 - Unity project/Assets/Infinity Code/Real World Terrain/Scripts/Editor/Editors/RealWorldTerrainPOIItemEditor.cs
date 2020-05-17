/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainPOIItem))]
    public class RealWorldTerrainPOIItemEditor : Editor
    {
        private SerializedProperty lng;
        private SerializedProperty lat;
        private SerializedProperty alt;
        private SerializedProperty title;

        private void CacheFields()
        {
            lng = serializedObject.FindProperty("x");
            lat = serializedObject.FindProperty("y");
            alt = serializedObject.FindProperty("altitude");
            title = serializedObject.FindProperty("title");
        }

        private void OnEnable()
        {
            CacheFields();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("To replace GameObject drag prefab here!!!", MessageType.Info);

            EditorGUILayout.PropertyField(title);
            EditorGUILayout.PropertyField(lat, new GUIContent("Latitude"));
            EditorGUILayout.PropertyField(lng, new GUIContent("Longitude"));
            EditorGUILayout.PropertyField(alt);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Update World Position")) UpdateWorldPosition();
            if (GUILayout.Button("Get Coordinates from World Position")) GetCoordinatesFromWorldPosition();

            ProcessEvents();
        }

        private void GetCoordinatesFromWorldPosition()
        {
            RealWorldTerrainPOIItem poi = target as RealWorldTerrainPOIItem;
            RealWorldTerrainContainer container = poi.GetComponentInParent<RealWorldTerrainContainer>();
            if (container == null)
            {
                Debug.LogWarning("Can not find RWT container");
                return;
            }

            double lng, lat, alt;
            container.GetCoordinatesByWorldPosition(poi.transform.position, out lng, out lat, out alt);
            poi.x = (float)lng;
            poi.y = (float)lat;
            poi.altitude = (float)alt;
        }

        private void UpdateWorldPosition()
        {
            RealWorldTerrainPOIItem poi = target as RealWorldTerrainPOIItem;
            RealWorldTerrainContainer container = poi.GetComponentInParent<RealWorldTerrainContainer>();
            if (container == null)
            {
                Debug.LogWarning("Can not find RWT container");
                return;
            }

            Vector3 pos;
            if (Math.Abs(poi.altitude) > float.Epsilon)
            {
                if (container.GetWorldPosition(poi.x, poi.y, poi.altitude, out pos)) poi.transform.position = pos;
            }
            else if (container.GetWorldPosition(poi.x, poi.y, out pos)) poi.transform.position = pos;
        }

        private void ProcessEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is GameObject)
                {
                    GameObject prefab = DragAndDrop.objectReferences[0] as GameObject;
                    int id = prefab.GetInstanceID();
                    if (id < 0) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    e.Use();
                }
            }
            else if (e.type == EventType.DragPerform)
            {
                if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is GameObject)
                {
                    GameObject prefab = DragAndDrop.objectReferences[0] as GameObject;
                    int id = prefab.GetInstanceID();
                    if (id < 0) return;

                    DragAndDrop.AcceptDrag();
                    e.Use();

                    GameObject go = Instantiate(prefab);
                    RealWorldTerrainPOIItem poi = target as RealWorldTerrainPOIItem;
                    go.transform.parent = poi.transform.parent;
                    go.name = poi.name;
                    go.transform.position = poi.transform.position;
                    go.transform.rotation = poi.transform.rotation;
                    go.transform.localScale = poi.transform.localScale;
                    RealWorldTerrainPOIItem n = go.AddComponent<RealWorldTerrainPOIItem>();
                    n.title = poi.title;
                    n.x = poi.x;
                    n.y = poi.y;
                    n.altitude = poi.altitude;
                    DestroyImmediate(poi.gameObject);
                }
            }
        }
    }
}