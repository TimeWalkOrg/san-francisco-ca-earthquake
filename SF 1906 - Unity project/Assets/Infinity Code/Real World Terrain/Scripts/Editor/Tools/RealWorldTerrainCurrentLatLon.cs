/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainCurrentLatLon : EditorWindow
    {
        private RealWorldTerrainContainer rwt;
        private Vector3 lastCursorPosition;
        private static RealWorldTerrainCurrentLatLon wnd;

        private void OnDestroy()
        {
            EditorApplication.update -= OnUpdate;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            wnd = null;
        }

        private void OnEnable()
        {
            OnDestroy();
            wnd = this;

            EditorApplication.update += OnUpdate;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        private void OnGUI()
        {
            rwt = (RealWorldTerrainContainer)EditorGUILayout.ObjectField("Real World Terrain", rwt, typeof(RealWorldTerrainContainer), true);

            if (rwt == null) return;

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            Vector3 cp = view.camera.transform.position;
            double longitude, latitude, altitude;

            rwt.GetCoordinatesByWorldPosition(cp, out longitude, out latitude, out altitude);
            
            EditorGUILayout.LabelField("Scene camera latitude: " + latitude);
            EditorGUILayout.LabelField("Scene camera longitude: " + longitude);
            EditorGUILayout.LabelField("Scene camera altitude: " + altitude);

            if (lastCursorPosition == Vector3.zero) return;

            rwt.GetCoordinatesByWorldPosition(lastCursorPosition, out longitude, out latitude, out altitude);

            EditorGUILayout.LabelField("Scene cursor latitude: " + latitude);
            EditorGUILayout.LabelField("Scene cursor longitude: " + longitude);
            EditorGUILayout.LabelField("Scene cursor altitude: " + altitude.ToString("F2") + " meters");
        }

        private void OnSceneGUI(SceneView view)
        {
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out hit)) lastCursorPosition = hit.point;
            else lastCursorPosition = Vector3.zero;
        }

        private void OnUpdate()
        {
            Repaint();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Current Position")]
        public static void OpenWindow()
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainCurrentLatLon>(false, "Current Position");
            wnd.rwt = FindObjectOfType<RealWorldTerrainContainer>();
        }

        public static void OpenWindow(RealWorldTerrainContainer container)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainCurrentLatLon>(false, "Current Position");
            wnd.rwt = container;
        }
    }

}