/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainPOIManager : EditorWindow
    {
        private static RealWorldTerrainPOIManager wnd;
        private RealWorldTerrainContainer container;
        private RealWorldTerrainPOIItem[] items;
        private Vector2 scrollPosition;

        private void OnDestroy()
        {
            wnd = null;
        }

        private void OnGUI()
        {
            container = EditorGUILayout.ObjectField("Container", container, typeof(RealWorldTerrainContainer), true) as RealWorldTerrainContainer;

            if (container == null) return;
            if (GUILayout.Button("Update POI") || items == null) UpdatePOI();

            GUILayout.Label("POI:");
            if (items == null || items.Length == 0)
            {
                GUILayout.Label("No POI.");
                return;
            }

            int index = 0;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (RealWorldTerrainPOIItem item in items)
            {
                index++;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(index + ": " + item.title);
                EditorGUILayout.LabelField("lat: " + item.y, GUILayout.Width(90));
                EditorGUILayout.LabelField("lng: " + item.x, GUILayout.Width(90));

                if (GUILayout.Button(new GUIContent("S", "Select GameObject"), GUILayout.ExpandWidth(false))) Selection.activeGameObject = item.gameObject;
                if (GUILayout.Button(new GUIContent("P", "Open in Object Placer"), GUILayout.ExpandWidth(false))) RealWorldTerrainObjectPlacerWindow.OpenWindow(container, item.x, item.y);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void UpdatePOI()
        {
            items = container.GetComponentsInChildren<RealWorldTerrainPOIItem>();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/POI Manager")]
        public static void OpenWindow()
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainPOIManager>(false, "POI Manager");
            wnd.container = FindObjectOfType<RealWorldTerrainContainer>();
        }
        public static void OpenWindow(RealWorldTerrainContainer container)
        {
            OpenWindow();
            wnd.container = container;
        }
    }
}