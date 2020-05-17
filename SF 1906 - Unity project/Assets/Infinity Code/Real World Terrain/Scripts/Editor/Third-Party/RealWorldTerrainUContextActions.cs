/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

#if UCONTEXT
using System.Diagnostics;
using InfinityCode.RealWorldTerrain.ExtraTypes;
using InfinityCode.RealWorldTerrain.JSON;
using InfinityCode.RealWorldTerrain.Tools;
using InfinityCode.uContext;
using InfinityCode.uContext.Actions;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InfinityCode.RealWorldTerrain.ThirdParty
{
    public class RealWorldTerrainUContextActions: ActionItem, IValidatableLayoutItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        private void Geocode()
        {
            uContextMenu.Close();
            InputDialog.Show("Input Location Name", "Location Name", OnInputLocationName);
        }

        protected override void Init()
        {
            Texture2D icon = RealWorldTerrainResources.GetIcon("RWT-uContext");
            _guiContent = new GUIContent(icon, "Real World Terrain");
        }

        public override void Invoke()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Find Location By Name"), false, Geocode);
            menu.AddItem(new GUIContent("Find Location Name Under Cursor"), false, ReverseGeocode);
            menu.AddItem(new GUIContent("Place Object"), false, ShowObjectPlacer);
            menu.AddItem(new GUIContent("Show Google Maps"), false, ShowGoogleMaps);
            menu.AddItem(new GUIContent("Show Google Street View"), false, ShowGoogleStreetView);
            menu.AddItem(new GUIContent("Show Open Street Map"), false, ShowOpenStreetMap);
            menu.ShowAsContext();
        }

        private void OnGeocodeComplete(RealWorldTerrainWWW www)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                return;
            }

            RealWorldTerrainJsonItem json = RealWorldTerrainJson.Parse(www.text);
            RealWorldTerrainJsonItem firstItem = json["candidates/0"];
            if (firstItem == null) return;

            RealWorldTerrainJsonItem location = firstItem["location"];

            double cx = location.V<double>("x"), cy = location.V<double>("y");

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            Vector3 worldPos;
            if (!container.GetWorldPosition(cx, cy, out worldPos)) return;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = firstItem.V<string>("address");
            go.transform.position = worldPos;

            Bounds bounds = go.GetComponent<Renderer>().bounds;
            bounds.size *= 5;
            SceneView.lastActiveSceneView.Frame(bounds);
        }

        private void OnInputLocationName(string locationName)
        {
            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            new RealWorldTerrainWWW(
                "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates?f=pjson&address=" + 
                RealWorldTerrainWWW.EscapeURL(locationName) + "&searchExtent=" + 
                container.leftLongitude.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," +
                container.topLatitude.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," +
                container.rightLongitude.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," +
                container.bottomLatitude.ToString(RealWorldTerrainCultureInfo.numberFormat)
            ).OnComplete += OnGeocodeComplete;
        }

        private void OnReverseGeocodeComplete(RealWorldTerrainWWW www)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                return;
            }

            RealWorldTerrainJsonItem json = RealWorldTerrainJson.Parse(www.text);
            Debug.Log(json["address/LongLabel"].V<string>());
        }

        private void ReverseGeocode()
        {
            uContextMenu.Close();

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            double lng, lat, alt;
            container.GetCoordinatesByWorldPosition(uContextMenu.lastWorldPosition, out lng, out lat, out alt);

            new RealWorldTerrainWWW(
                "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode?f=pjson&location=" + lng.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," + lat.ToString(RealWorldTerrainCultureInfo.numberFormat)
            ).OnComplete += OnReverseGeocodeComplete;
        }

        private void ShowGoogleMaps()
        {
            uContextMenu.Close();

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            double lng, lat;
            if (!container.GetCoordinatesByWorldPosition(uContextMenu.lastWorldPosition, out lng, out lat)) return;

            Process.Start("https://www.google.com/maps/@?api=1&map_action=map&basemap=satellite&zoom=19&center=" + lat.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," + lng.ToString(RealWorldTerrainCultureInfo.numberFormat));
        }

        private void ShowGoogleStreetView()
        {
            uContextMenu.Close();

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            double lng, lat;
            if (!container.GetCoordinatesByWorldPosition(uContextMenu.lastWorldPosition, out lng, out lat)) return;

            Process.Start("https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=" + lat.ToString(RealWorldTerrainCultureInfo.numberFormat) + "," + lng.ToString(RealWorldTerrainCultureInfo.numberFormat));
        }

        private void ShowObjectPlacer()
        {
            uContextMenu.Close();

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            double lng, lat;
            if (!container.GetCoordinatesByWorldPosition(uContextMenu.lastWorldPosition, out lng, out lat)) return;

            RealWorldTerrainObjectPlacerWindow.OpenWindow(container, lng, lat);
        }

        private void ShowOpenStreetMap()
        {
            uContextMenu.Close();

            RealWorldTerrainContainer container = Object.FindObjectOfType<RealWorldTerrainContainer>();
            if (container == null) return;

            double lng, lat;
            if (!container.GetCoordinatesByWorldPosition(uContextMenu.lastWorldPosition, out lng, out lat)) return;

            Process.Start(string.Format(RealWorldTerrainCultureInfo.numberFormat, "http://www.openstreetmap.org/#map={0}/{1}/{2}", 19, lat, lng));
        }

        public bool Validate()
        {
            return Object.FindObjectOfType<RealWorldTerrainContainer>() != null;
        }
    }
}
#endif