/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainObjectPlacerWindow : EditorWindow
    {
        private static string[] gridLabels = { "Place New Object", "Update Position" };

        private static RealWorldTerrainObjectPlacerWindow wnd;
        private int isNewGameobject = 0;
        private GameObject obj;
        private double lat;
        private double lng;
        private RealWorldTerrainContainer container;
        private bool selectGameObject = true;

        private bool hasCoordinates = false;
        private double cursorLongitude;
        private double cursorLatitude;
        private double cursorAltitude;

        private void OnCoordinatesGUI()
        {
            lat = EditorGUILayout.DoubleField("Latitude", lat);
            lng = EditorGUILayout.DoubleField("Longitude", lng);
        }

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
            EditorGUI.BeginChangeCheck();
            isNewGameobject = GUILayout.SelectionGrid(isNewGameobject, gridLabels, 2);
            if (EditorGUI.EndChangeCheck()) obj = null;

            container = EditorGUILayout.ObjectField("Container", container, typeof(RealWorldTerrainContainer), true) as RealWorldTerrainContainer;

            if (isNewGameobject == 0) OnNewGUI();
            else OnUpdateGUI();

            if (hasCoordinates)
            {
                EditorGUILayout.LabelField("Cursor Coordinates:");
                EditorGUILayout.LabelField("Latitude: ", cursorLatitude.ToString());
                EditorGUILayout.LabelField("Longitude: ", cursorLongitude.ToString());
                EditorGUILayout.LabelField("Altitude: ", cursorAltitude.ToString("F2") + " meters");
                EditorGUILayout.LabelField("Use CTRL+SHIFT to insert the coordinates.");

                if (Event.current.control && Event.current.shift)
                {
                    lat = (float)cursorLatitude;
                    lng = (float)cursorLongitude;
                }
            }
        }

        private void OnNewGUI()
        {
            obj = EditorGUILayout.ObjectField("Prefab: ", obj, typeof(GameObject), true) as GameObject;

            OnCoordinatesGUI();
            selectGameObject = EditorGUILayout.Toggle("Select Gameobject?", selectGameObject);

            if (GUILayout.Button("Place") && ValidateFields())
            {
                GameObject go = Instantiate(obj) as GameObject;
                UpdateGameObjectPosition(go);
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (container == null) return;

            Vector2 mp = Event.current.mousePosition;
            mp.y = view.camera.pixelHeight - mp.y;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            hasCoordinates = Physics.Raycast(ray, out hit);
            if (hasCoordinates) container.GetCoordinatesByWorldPosition(hit.point, out cursorLongitude, out cursorLatitude, out cursorAltitude);
        }

        private void OnUpdate()
        {
            Repaint();
        }

        private void OnUpdateGUI()
        {
            obj = EditorGUILayout.ObjectField("GameObject: ", obj, typeof(GameObject), true) as GameObject;

            OnCoordinatesGUI();
            selectGameObject = EditorGUILayout.Toggle("Select Gameobject?", selectGameObject);

            if (GUILayout.Button("Update") && ValidateFields())
            {
                UpdateGameObjectPosition(obj);
            }
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Object Placer")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(RealWorldTerrainContainer container)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainObjectPlacerWindow>(false, "Object Placer", true);
            if (container == null) wnd.container = FindObjectOfType<RealWorldTerrainContainer>();
            else wnd.container = container;
        }

        public static void OpenWindow(RealWorldTerrainContainer container, double lng, double lat)
        {
            OpenWindow(container);
            wnd.lat = lat;
            wnd.lng = lng;
        }

        private void SelectGameObject(GameObject go)
        {
            if (!selectGameObject) return;

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private static void ShowError(string message)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        private void UpdateGameObjectPosition(GameObject go)
        {
            Vector3 worldPosition;
            if (container.GetWorldPosition(lng, lat, out worldPosition))
            {
                go.transform.position = worldPosition;
                SelectGameObject(go);
            }
        }

        private bool ValidateFields()
        {
            if (container == null)
            {
                ShowError("Please select Real World Terrain Container.");
                return false;
            }

            if (obj == null)
            {
                ShowError(string.Format("Please select {0}.", isNewGameobject == 0 ? "Prefab" : "GameObject"));
                return false;
            }

            if (!container.Contains(lng, lat))
            {
                ShowError("These the coordinates outside terrain.");
                return false;
            }

            return true;
        }
    }
}