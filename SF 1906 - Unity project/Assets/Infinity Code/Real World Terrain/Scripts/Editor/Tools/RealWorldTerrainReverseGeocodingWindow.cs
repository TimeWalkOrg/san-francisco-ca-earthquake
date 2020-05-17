/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Webservices;
using InfinityCode.RealWorldTerrain.Webservices.Results;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainReverseGeocodingWindow: EditorWindow
    {
        private static RealWorldTerrainReverseGeocodingWindow wnd;
        private RealWorldTerrainMonoBase target;
        private double cursorLatitude;
        private double cursorLongitude;
        private double cursorAltitude;
        private float lat;
        private float lng;
        private bool hasCoordinates = false;
        private string languageCode = "en";

        private string formattedAddress;
        private string response;
        private Vector2 scrollPosition;
        private string key;

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
            EditorApplication.update += OnUpdate;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Container", target, typeof(RealWorldTerrainContainer), true) as RealWorldTerrainContainer;

            lat = EditorGUILayout.FloatField("Latitude", lat);
            lng = EditorGUILayout.FloatField("Longitude", lng);
            key = EditorGUILayout.TextField("Google API key", key);

            EditorGUILayout.BeginHorizontal();
            languageCode = EditorGUILayout.TextField("Language Code", languageCode);
            RealWorldTerrainGUIUtils.DrawHelpButton("List of Languages", "https://developers.google.com/maps/faq?hl=en#languagesupport");
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("What's here?"))
            {
                RealWorldTerrainGoogleGeocoding.Find(
                    new RealWorldTerrainGoogleGeocoding.ReverseGeocodingParams(lng, lat)
                    {
                        language = languageCode,
                        key = key
                    }
                ).OnComplete += OnRequestComplete;
            }

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

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(formattedAddress))
            {
                GUILayout.Label("Formatted Address: " + formattedAddress, EditorStyles.wordWrappedLabel);
            }

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
            Debug.Log(response);
            this.response = response;
            try
            {
                RealWorldTerrainGoogleGeocodingResult[] result = RealWorldTerrainGoogleGeocoding.GetResults(response);
                if (result.Length > 0)
                {
                    formattedAddress = result[0].formatted_address;
                }
            }
            catch
            {
                
            }
            
        }

        private void OnSceneGUI(SceneView view)
        {
            if (target == null) return;

            Vector2 mp = Event.current.mousePosition;
            mp.y = view.camera.pixelHeight - mp.y;

            hasCoordinates = target.GetCoordinatesByScreenPosition(mp, out cursorLongitude, out cursorLatitude, out cursorAltitude, view.camera);
        }

        private void OnUpdate()
        {
            Repaint();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Reverse Geocoder")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }


        public static void OpenWindow(RealWorldTerrainMonoBase target)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainReverseGeocodingWindow>(true, "Reverse Geocoder");
            if (target == null) wnd.target = FindObjectOfType<RealWorldTerrainContainer>();
            else wnd.target = target;
        }
    }
}
