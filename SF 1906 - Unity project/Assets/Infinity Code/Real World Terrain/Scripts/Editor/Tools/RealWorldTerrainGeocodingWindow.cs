/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Webservices;
using InfinityCode.RealWorldTerrain.Webservices.Results;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainGeocodingWindow : EditorWindow
    {
        private static RealWorldTerrainGeocodingWindow wnd;
        private RealWorldTerrainMonoBase target;

        private string response;
        private Vector2 scrollPosition;
        private string address;
        private string languageCode = "en";
        private string key;
        private GameObject resultGameObject;

        private void OnDestroy()
        {
            wnd = null;
        }

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Container", target, typeof(RealWorldTerrainContainer), true) as RealWorldTerrainContainer;

            address = EditorGUILayout.TextField("Location Name", address);
            key = EditorGUILayout.TextField("Google API key", key);
            EditorGUILayout.BeginHorizontal();
            languageCode = EditorGUILayout.TextField("Language Code", languageCode);
            RealWorldTerrainGUIUtils.DrawHelpButton("List of Languages", "https://developers.google.com/maps/faq?hl=en#languagesupport");
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Search"))
            {
                RealWorldTerrainGoogleGeocoding.Find(
                    new RealWorldTerrainGoogleGeocoding.GeocodingParams(address)
                    {
                        language = languageCode,
                        key = key
                    }
                ).OnComplete += OnRequestComplete;
            }

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(response))
            {
                GUILayout.Label("Full Response: ");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                EditorGUILayout.TextArea(response);
                EditorGUILayout.EndScrollView();
            }
        }

        private void OnRequestComplete(string response)
        {
            this.response = response;
            try
            {
                RealWorldTerrainGoogleGeocodingResult[] result = RealWorldTerrainGoogleGeocoding.GetResults(response);
                if (result.Length > 0)
                {
                    Vector3 pos;
                    target.GetWorldPosition(result[0].geometry_location, out pos);
                    if (pos != default(Vector3))
                    {
                        resultGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        resultGameObject.name = "Geocoding Result";
                        resultGameObject.transform.position = pos;
                        resultGameObject.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
                        RealWorldTerrainGeocodingObject geocodingObject = resultGameObject.AddComponent<RealWorldTerrainGeocodingObject>();
                        geocodingObject.info = result[0];
#if UNITY_2019_1_OR_NEWER
                        SceneView.duringSceneGui += OnSceneGUI;
#else
                        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
                    }
                }
            }
            catch
            {

            }
            Repaint();
        }

        private void OnSceneGUI(SceneView view)
        {
            view.LookAt(resultGameObject.transform.position, view.camera.transform.rotation, 100);
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Geocoder")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainGeocodingWindow>(true, "Geocoder");
            if (target == null) wnd.target = FindObjectOfType<RealWorldTerrainContainer>();
            else wnd.target = target;
        }
    }
}