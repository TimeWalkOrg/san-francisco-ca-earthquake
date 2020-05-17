/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using InfinityCode.RealWorldTerrain.Editors;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Phases;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if BUILDR2
using BuildR2;
#endif

#if EASYROADS3D
using EasyRoads3Dv3;
#endif

#if PROCEDURAL_TOOLKIT
using ProceduralToolkit.Buildings;
#endif

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainGUIUtils
    {
        private const int LabelWidth = 170;

#region Fields

        public static int iProgress;
        public static string phasetitle = "";

        private static string bingAPI;
        private static string mapboxAPI;
        private static string earthDataLogin;
        private static string earthDataPass;
        private static Vector2 scrollPos = Vector2.zero;
        private static bool showCoordinates = true;
        private static bool showBuildRCustomPresets;
        private static bool showCustomProviderTokens;
        private static bool showPOI;
        private static bool showTerrains = true;
        private static bool showTextures = true;

        private static readonly string[] labels2n = { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        private static readonly int[] values2n = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private static readonly string[] labels2n1 = { "33", "65", "129", "257", "513", "1025", "2049", "4097" };
        private static readonly int[] values2n1 = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        private static readonly string[] labels2n4 = { "8", "16", "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        private static readonly int[] values2n4 = { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private static readonly string[] labelsBaseMapRes = { "16", "32", "64", "128", "256", "512", "1024", "2048"};
        private static readonly int[] valuesBaseMapRes = { 16, 32, 64, 128, 256, 512, 1024, 2048};
        private static readonly string[] labelsTextureSize =
        {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
        };
        private static readonly int[] valuesTextureSize =
        {
            32, 64, 128, 256, 512, 1024, 2048, 4096, 8192
        };

        private static int providerIndex;
        private static string[] providersTitle;
        private static RealWorldTerrainTextureProviderManager.Provider[] providers;
        private static bool showElevationProvider = true;
        public static GUIStyle oddStyle;
        public static GUIStyle evenStyle;
        private static string[] roadTypeNames;

#if EASYROADS3D
        private static ERModularBase roadNetwork;
        private static bool needFindRoadNetwork = true;
        private static string[] erRoadTypeNames;
#endif


        #endregion

        #region Properties

        public static RealWorldTerrainGenerateType generateType
        {
            get { return RealWorldTerrainWindow.generateType; }
        }

        private static RealWorldTerrainPhase phase
        {
            get { return RealWorldTerrainPhase.activePhase; }
        }

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        private static RealWorldTerrainWindow wnd
        {
            get { return RealWorldTerrainWindow.wnd; }
        }

        #endregion

        #region Methods

        #region General

        public static double DoubleField(string label, double value, string tooltip, string href = "")
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.DoubleField(label, value);

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.helpIcon, tooltip),
                RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false)))
            {
                if (href != "") Application.OpenURL(href);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static void DrawHelpButton(string tooltip, string href = null)
        {
            GUIContent content = new GUIContent(RealWorldTerrainResources.helpIcon, tooltip);
            if (GUILayout.Button(content, RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false))
                && !string.IsNullOrEmpty(href))
            {
                Application.OpenURL(href);
            }
        }

        public static float FloatField(string label, float value, string tooltip, string href = "")
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.FloatField(label, value);

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.helpIcon, tooltip),
                RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false)))
            {
                if (href != "") Application.OpenURL(href);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        public static bool Foldout(bool value, string text)
        {
            return GUILayout.Toggle(value, text, EditorStyles.foldout);
        }

        public static int IntPopup(string label, int value, string[] displayedOptions, int[] optionValues, string tooltip, string href)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntPopup(label, value, displayedOptions, optionValues);

            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static int IntPopup(string label, int value, string[] displayedOptions, int[] optionValues, string tooltip, string[] hrefs)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntPopup(label, value, displayedOptions, optionValues);
            string href = null;
            if (hrefs != null && hrefs.Length > value) href = hrefs[value];
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static int IntField(string label, int value, string tooltip, string href = null)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.IntField(label, value);
            DrawHelpButton(tooltip, href);
            GUILayout.EndHorizontal();
            return value;
        }

        public static void OnGUI()
        {
#if !UNITY_WEBPLAYER
            if (!RealWorldTerrainWindow.isCapturing) OnIdleGUI();
            else OnGenerate();
#else
            WebplayerWarning();
#endif
        }

        private static bool Toggle(bool value, string text, string tooltip = null, string href = null)
        {
            EditorGUILayout.BeginHorizontal();

            value = GUILayout.Toggle(value, text);
            DrawHelpButton(tooltip, href);
            EditorGUILayout.EndHorizontal();

            return value;
        }

        private static void WebplayerWarning()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
            GUILayout.Label("Real World Terrain can not work in a WebPlayer mode.\nSelect \"File / Build Settings\" to select another platform.", style);
            if (GUILayout.Button("Switch to Standalone"))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            }
        }

#endregion

#region Toolbar

        private static void Toolbar()
        {
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("File", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Import Prefs"), false, () =>
                {
                    string filename = EditorUtility.OpenFilePanel("Import Prefs", Application.dataPath, "xml");
                    if (!string.IsNullOrEmpty(filename)) prefs.LoadFromXML(filename);
                });
                menu.AddItem(new GUIContent("Export Prefs"), false, () =>
                {
                    string filename = EditorUtility.SaveFilePanel("Import Prefs", Application.dataPath, "Prefs", "xml");
                    if (!string.IsNullOrEmpty(filename)) File.WriteAllText(filename, prefs.ToXML(new XmlDocument()).OuterXml, Encoding.UTF8);
                });
                menu.ShowAsContext();
            }

            if (GUILayout.Button("History", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                RealWorldTerrainHistoryWindow.OpenWindow();
            }

            if (RealWorldTerrainUpdaterWindow.hasNewVersion)
            {
                Color defColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                if (GUILayout.Button("New version available!!! Click here to update.", buttonStyle))
                {
                    wnd.Close();
                    RealWorldTerrainUpdaterWindow.OpenWindow();
                }
                GUI.backgroundColor = defColor;
            }
            else
                GUILayout.Label("", buttonStyle);

            if (GUILayout.Button("Settings", buttonStyle, GUILayout.ExpandWidth(false))) RealWorldTerrainSettingsWindow.OpenWindow();

            if (GUILayout.Button("Help", buttonStyle, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Documentation"), false, ViewDocs);
                menu.AddItem(new GUIContent("Product Page"), false, ProductPage);
                menu.AddItem(new GUIContent("Support"), false, SendMail);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Check Updates"), false, RealWorldTerrainUpdaterWindow.OpenWindow);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("About"), false, RealWorldTerrainAboutWindow.OpenWindow);
                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private static void ProductPage()
        {
            Process.Start("http://infinity-code.com/products/real-world-terrain");
        }

        public static void RunHelper()
        {
            string helperPath = "file://" + Directory.GetFiles(Application.dataPath, "RWT_Helper.html", SearchOption.AllDirectories)[0].Replace('\\', '/');
            if (Application.platform == RuntimePlatform.OSXEditor) helperPath = helperPath.Replace(" ", "%20");
            prefs.Save();
            Application.OpenURL(helperPath);
        }

        private static void SendMail()
        {
            Process.Start("mailto:support@infinity-code.com?subject=Real%20World%20Terrain");
        }

        private static void ViewDocs()
        {
            Process.Start("http://infinity-code.com/docs/real-world-terrain");
        }

#endregion

#region Idle

#region General

        private static void Coordinates()
        {
            prefs.title = EditorGUILayout.TextField("Title", prefs.title);
            GUILayout.Space(10);

            GUILayout.Label("Top-Left");
            EditorGUI.indentLevel++;
            prefs.topLatitude = DoubleField("Latitude", prefs.topLatitude, "Latitude of the Top-Left corner of the area. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
            prefs.leftLongitude = DoubleField("Longitude", prefs.leftLongitude, "Longitude of the Top-Left corner of the area. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            GUILayout.Label("Bottom-Right");
            EditorGUI.indentLevel++;
            prefs.bottomLatitude = DoubleField("Latitude", prefs.bottomLatitude, "Latitude of the Bottom-Right corner of the area. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
            prefs.rightLongitude = DoubleField("Longitude", prefs.rightLongitude, "Longitude of the Bottom-Right corner of the area. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            prefs.useAnchor = Toggle(prefs.useAnchor, "Use Anchor", "The coordinates of the anchor point at which there will be a zero position in the scene.");
            if (prefs.useAnchor)
            {
                prefs.anchorLatitude = DoubleField("Latitude", prefs.anchorLatitude, "Latitude of the Anchor. \nValues: -90 to 90.", "http://en.wikipedia.org/wiki/Latitude");
                prefs.anchorLongitude = DoubleField("Longitude", prefs.anchorLongitude, "Longitude of the Anchor. \nValues: -180 to 180.", "http://en.wikipedia.org/wiki/Longitude");
            }

            GUI.SetNextControlName("InsertCoordsButton");
            if (GUILayout.Button("Insert the coordinates from the clipboard")) InsertCoords();
            if (GUILayout.Button("Run the helper")) RunHelper();
            if (prefs.resultType == RealWorldTerrainResultType.terrain && GUILayout.Button("Get the best settings for the specified coordinates")) RealWorldTerrainSettingsGeneratorWindow.OpenWindow();
            if (GUILayout.Button("Show Open Street Map"))
            {
                Vector2 center;
                int zoom;
                RealWorldTerrainUtils.GetCenterPointAndZoom(new []{prefs.leftLongitude, prefs.topLatitude, prefs.rightLongitude, prefs.bottomLatitude}, out center, out zoom);
                Process.Start(string.Format(RealWorldTerrainCultureInfo.numberFormat, "http://www.openstreetmap.org/#map={0}/{1}/{2}", zoom, center.y, center.x));
            }

            GUILayout.Space(10);
        }

        public static void InsertCoords()
        {
            GUI.FocusControl("InsertCoordsButton");
            string nodeStr = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(nodeStr)) return;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(nodeStr);
                XmlNode node = doc.FirstChild;
                if (node.Name != "Coords" || node.Attributes == null) return;

                prefs.leftLongitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "tlx");
                prefs.topLatitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "tly");
                prefs.rightLongitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "brx");
                prefs.bottomLatitude = RealWorldTerrainXMLExt.GetAttribute<float>(node, "bry");

                if (prefs.useAnchor)
                {
                    if (EditorUtility.DisplayDialog("Remove Anchor", "Remove anchor point from previous settings?", "Remove", "Keep"))
                    {
                        prefs.useAnchor = false;
                        double mx1, my1, mx2, my2;
                        RealWorldTerrainUtils.LatLongToMercat(prefs.leftLongitude, prefs.topLatitude, out mx1, out my1);
                        RealWorldTerrainUtils.LatLongToMercat(prefs.rightLongitude, prefs.bottomLatitude, out mx2, out my2);
                        mx1 = (mx2 + mx1) / 2;
                        my1 = (my2 + my1) / 2;
                        RealWorldTerrainUtils.MercatToLatLong(mx1, my1, out prefs.anchorLongitude, out prefs.anchorLatitude);
                    }
                }

                XmlNodeList POInodes = node.SelectNodes("//POI");
                prefs.POI = new List<RealWorldTerrainPOI>();
                foreach (XmlNode n in POInodes) prefs.POI.Add(new RealWorldTerrainPOI(n));

                prefs.Save();
            }
            catch { }
        }

        private static void OnIdleGUI()
        {
            Toolbar();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUIUtility.labelWidth = LabelWidth;

            bool isFull = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.full;
            bool isTerrain = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain;
            bool isTexture = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.texture;
            bool isAdditional = isFull || RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.additional;
            bool isRawOutput = prefs.resultType == RealWorldTerrainResultType.gaiaStamp || prefs.resultType == RealWorldTerrainResultType.rawFile;

            if (isFull)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                showCoordinates = EditorGUILayout.Foldout(showCoordinates, "Decimal coordinates");
                if (showCoordinates) Coordinates();
                EditorGUILayout.EndVertical();
            }

            if (isTerrain)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                showTerrains = EditorGUILayout.Foldout(showTerrains, "Terrains");

                if (showTerrains) TerrainGUI();

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                showElevationProvider = EditorGUILayout.Foldout(showElevationProvider, "Elevation Provider");

                if (showElevationProvider)
                {
                    prefs.elevationProvider = (RealWorldTerrainElevationProvider)EditorGUILayout.EnumPopup(prefs.elevationProvider);
                    TerrainProviderExtraFields();
                }

                EditorGUILayout.EndVertical();
            }

            if (isTexture && !isRawOutput)
            {
                if (isFull)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();
                    if (prefs.generateTextures) showTextures = GUILayout.Toggle(showTextures, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));
                    prefs.generateTextures = GUILayout.Toggle(prefs.generateTextures, "Textures");
                    EditorGUILayout.EndHorizontal();
                    if (showTextures && prefs.generateTextures) TextureGUI();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField("Textures");
                    prefs.generateTextures = true;

                    TextureGUI();

                    EditorGUILayout.EndVertical();
                }
            }

            if (isFull && !isRawOutput)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                showPOI = EditorGUILayout.Foldout(showPOI, "POI");
                DrawHelpButton("Here you can specify a point of interest, which will be created on the terrains.");
                EditorGUILayout.EndHorizontal();
                if (showPOI) POI();
                EditorGUILayout.EndVertical();
            }

            if (isAdditional && !isRawOutput) OSMGUI();

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start")) RealWorldTerrainWindow.StartCapture();
            if (GUILayout.Button("Memory Usage", GUILayout.ExpandWidth(false))) RealWorldTerrainMemoryUsageWindow.OpenWindow();
            if (GUILayout.Button("Clear Cache", GUILayout.ExpandWidth(false))) RealWorldTerrainWindow.ClearCache();
            GUILayout.EndHorizontal();
        }

#endregion

#region Terrain

        private static void BingMapsElevationExtraFields()
        {
            if (bingAPI == null) bingAPI = RealWorldTerrainPrefs.LoadPref("BingAPI", "");
            EditorGUILayout.HelpBox("Public Windows App or Public Windows Phone App have the 50.000 transaction within 24 hours. With the other chooses there's only 125.000 transactions within a year and the key will expire if exceeding it.", MessageType.Info);
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            bingAPI = EditorGUILayout.TextField("Bing Maps API key", bingAPI);
            if (EditorGUI.EndChangeCheck())
            {
                if (bingAPI == "") RealWorldTerrainPrefs.DeletePref("BingAPI");
                else RealWorldTerrainPrefs.SetPref("BingAPI", bingAPI);
            }

            if (string.IsNullOrEmpty(bingAPI))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            if (GUILayout.Button("Create Key", GUILayout.ExpandWidth(false))) Process.Start("http://msdn.microsoft.com/en-us/library/ff428642.aspx");
            GUILayout.EndHorizontal();
        }

        private static void MapboxElevationExtraFields()
        {
            if (mapboxAPI == null) mapboxAPI = RealWorldTerrainPrefs.LoadPref("MapboxAPI", "");
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            mapboxAPI = EditorGUILayout.TextField("Mapbox API key", mapboxAPI);
            if (EditorGUI.EndChangeCheck())
            {
                if (mapboxAPI == "") RealWorldTerrainPrefs.DeletePref("MapboxAPI");
                else RealWorldTerrainPrefs.SetPref("MapboxAPI", mapboxAPI);
            }

            if (string.IsNullOrEmpty(mapboxAPI))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Get Mapbox API key")) Process.Start("https://www.mapbox.com/studio/account/tokens/");
            GUILayout.Space(10);
        }

        private static void SRTM30ExtraFields()
        {
            if (earthDataLogin == null || earthDataPass == null)
            {
                earthDataLogin = RealWorldTerrainPrefs.LoadPref("EarthDataLogin", "");
                earthDataPass = RealWorldTerrainPrefs.LoadPref("EarthDataPass", "");
            }
            
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            earthDataLogin = EditorGUILayout.TextField("EarthData username", earthDataLogin);
            if (EditorGUI.EndChangeCheck())
            {
                if (earthDataLogin == "") RealWorldTerrainPrefs.DeletePref("EarthDataLogin");
                else RealWorldTerrainPrefs.SetPref("EarthDataLogin", earthDataLogin);
            }

            if (string.IsNullOrEmpty(earthDataLogin))
            {
                GUILayout.Box(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            earthDataPass = EditorGUILayout.TextField("EarthData password", earthDataPass);
            if (EditorGUI.EndChangeCheck())
            {
                if (earthDataPass == "") RealWorldTerrainPrefs.DeletePref("EarthDataPass");
                else RealWorldTerrainPrefs.SetPref("EarthDataPass", earthDataPass);
            }

            if (string.IsNullOrEmpty(earthDataPass))
            {
                GUILayout.Button(new GUIContent(RealWorldTerrainResources.warningIcon, "Required"), RealWorldTerrainResources.helpStyle, GUILayout.ExpandWidth(false));
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Register on EarthData", GUILayout.ExpandWidth(true))) Process.Start("https://urs.earthdata.nasa.gov/users/new");
            GUILayout.Space(10);
        }

        private static void TerrainDataSettings()
        {
            if (values2n1.All(v => v != prefs.heightmapResolution))
            {
                prefs.heightmapResolution = Mathf.ClosestPowerOfTwo(prefs.heightmapResolution) + 1;
                if (values2n1.All(v => v != prefs.heightmapResolution)) prefs.heightmapResolution = 129;
            }

            prefs.heightmapResolution =
                IntPopup("Height Map Resolution", prefs.heightmapResolution, labels2n1, values2n1,
                    "The HeightMap resolution for each Terrain.", "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.detailResolution = IntField("Detail Resolution", prefs.detailResolution,
                "The resolution of the map that controls grass and detail meshes. For performance reasons (to save on draw calls) the lower you set this number the better.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.detailResolution = Mathf.Clamp(prefs.detailResolution, 32, 4096);

            prefs.resolutionPerPatch = IntPopup("Resolution Per Patch", prefs.resolutionPerPatch, labels2n4, values2n4,
                "Specifies the size in pixels of each individually rendered detail patch. A larger number reduces draw calls, but might increase triangle count since detail patches are culled on a per batch basis. A recommended value is 16. If you use a very large detail object distance and your grass is very sparse, it makes sense to increase the value.",
                "http://docs.unity3d.com/Documentation/ScriptReference/TerrainData.SetDetailResolution.html");

            if (prefs.detailResolution % prefs.resolutionPerPatch != 0)
                prefs.detailResolution = prefs.detailResolution / prefs.resolutionPerPatch * prefs.resolutionPerPatch;

            prefs.controlTextureResolution = IntPopup("Control Texture Resolution", prefs.controlTextureResolution, labels2n, values2n,
                "Resolution of the splatmap that controls the blending of the different terrain textures.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            prefs.baseMapResolution = IntPopup("Base Map Resolution", prefs.baseMapResolution, labelsBaseMapRes, valuesBaseMapRes,
                "The resolution of the composite texture that is used in place of the splat map at certain distances.",
                "http://docs.unity3d.com/Documentation/Components/terrain-UsingTerrains.html");

            if (!valuesBaseMapRes.Contains(prefs.baseMapResolution)) prefs.baseMapResolution = 1024;
        }

        private static void TerrainFullFields()
        {
            prefs.resultType = (RealWorldTerrainResultType) EditorGUILayout.EnumPopup("Result", prefs.resultType);

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Count terrains.    X");
                prefs.terrainCount.x = Mathf.Max(EditorGUILayout.IntField(prefs.terrainCount.x), 1);
                GUILayout.Label("Y");
                prefs.terrainCount.y = Mathf.Max(EditorGUILayout.IntField(prefs.terrainCount.y), 1);
                GUILayout.EndHorizontal();
            }

            if (prefs.resultType == RealWorldTerrainResultType.gaiaStamp)
            {
#if !GAIA_PRESENT
                EditorGUILayout.HelpBox("Gaia not found. Import Gaia into the project.", MessageType.Error);
#endif
                prefs.gaiaStampResolution = EditorGUILayout.IntField("Stamp Resolution", prefs.gaiaStampResolution);
            }
            else if (prefs.resultType == RealWorldTerrainResultType.rawFile)
            {
                EditorGUILayout.BeginHorizontal();
                prefs.rawFilename = EditorGUILayout.TextField("Filename", prefs.rawFilename);
                if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(null);
                    string ext = (prefs.rawType == RealWorldTerrainRawType.RAW) ? "raw" : "png";
                    string rawFilename = EditorUtility.SaveFilePanel("Output RAW filename", Application.dataPath, "terrain." + ext, ext);
                    if (!string.IsNullOrEmpty(rawFilename)) prefs.rawFilename = rawFilename;
                }
                EditorGUILayout.EndHorizontal();
                prefs.rawType = (RealWorldTerrainRawType)EditorGUILayout.EnumPopup("Type", prefs.rawType);
                if (prefs.rawType == RealWorldTerrainRawType.RAW) prefs.rawByteOrder = (RealWorldTerrainByteOrder)EditorGUILayout.EnumPopup("Byte order", prefs.rawByteOrder);
                prefs.rawWidth = EditorGUILayout.IntField("Width", prefs.rawWidth);
                prefs.rawHeight = EditorGUILayout.IntField("Height", prefs.rawHeight);
            }
            else
            {
                prefs.sizeType = IntPopup(
                    "Size type",
                    prefs.sizeType, new[] {"Real world sizes", "Mercator sizes", "Fixed size"},
                    new[] {0, 1, 2},
                    "Specifies whether the projection will be determined by the size of the area.",
                    new[] {"http://en.wikipedia.org/wiki/Cylindrical_equal-area_projection", "http://en.wikipedia.org/wiki/Mercator_projection"}
                );

                if (prefs.sizeType == 2)
                {
                    prefs.fixedTerrainSize.x = EditorGUILayout.FloatField("Terrain Width", prefs.fixedTerrainSize.x);
                    prefs.fixedTerrainSize.z = EditorGUILayout.FloatField("Terrain Length", prefs.fixedTerrainSize.z);
                    prefs.terrainScale.y = EditorGUILayout.FloatField("Scale Y", prefs.terrainScale.y);
                }
                else prefs.terrainScale = EditorGUILayout.Vector3Field("Scale", prefs.terrainScale);
            }

            if (prefs.resultType != RealWorldTerrainResultType.rawFile)
            {
                prefs.elevationRange = (RealWorldTerrainElevationRange) EditorGUILayout.EnumPopup("Elevation range", prefs.elevationRange);
                if (prefs.elevationRange == RealWorldTerrainElevationRange.autoDetect)
                {
                    prefs.autoDetectElevationOffset.x = EditorGUILayout.FloatField("Min elevation offset", prefs.autoDetectElevationOffset.x);
                    prefs.autoDetectElevationOffset.y = EditorGUILayout.FloatField("Max elevation offset", prefs.autoDetectElevationOffset.y);
                }
                else if (prefs.elevationRange == RealWorldTerrainElevationRange.fixedValue)
                {
                    prefs.fixedMinElevation = EditorGUILayout.FloatField("Min elevation", prefs.fixedMinElevation);
                    prefs.fixedMaxElevation = EditorGUILayout.FloatField("Max elevation", prefs.fixedMaxElevation);
                }
            }
        }

        private static void TerrainGUI()
        {
            if (generateType == RealWorldTerrainGenerateType.full) TerrainFullFields();

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                prefs.generateUnderWater = EditorGUILayout.Toggle("Generate Underwater", prefs.generateUnderWater);

                if (prefs.generateUnderWater)
                {
                    EditorGUI.indentLevel++;
                    int nodata = IntField(
                        "Max Underwater Depth",
                        prefs.nodataValue,
                        "SRTM v4.1 does not contain data on the underwater depths. Real World Terrain generates it by closest known areas of land. \nSpecify a value relative to sea level. \nFor example, if you want to get a depth of 200 meters, set the value \"-200\"."
                    );
                    if (nodata < short.MinValue) nodata = short.MinValue;
                    if (nodata > short.MaxValue) nodata = short.MaxValue;
                    prefs.depthSharpness = FloatField(
                        "Depth Sharpness",
                        prefs.depthSharpness,
                        "Escarpment of the seabed. \nGreater value - steeper slope.\nUnknown Value = Average Neighbor Known Values - Depth Sharpness."
                    );
                    if (prefs.depthSharpness < 0) prefs.depthSharpness = 0;
                    if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps) prefs.bingMapsUseZeroAsUnknown = EditorGUILayout.Toggle("Zero Elevation is Unknown", prefs.bingMapsUseZeroAsUnknown);
                    prefs.nodataValue = (short) nodata;
                    EditorGUI.indentLevel--;
                }
            }
            else prefs.generateUnderWater = false;
            
            
            if (prefs.resultType == RealWorldTerrainResultType.terrain) TerrainDataSettings();
            else if (prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                prefs.heightmapResolution =
                    RealWorldTerrainUtils.Limit(IntField("Height Map Resolution", prefs.heightmapResolution,
                        "Total HeightMap resolution for all Meshes."), 32, 65536);
                if (prefs.heightmapResolution > 254) EditorGUILayout.HelpBox("Several meshes will be generated, a maximum of 65,000 vertices in each.", MessageType.Info);
                if (prefs.generateTextures && prefs.textureCount > 1) EditorGUILayout.HelpBox("Several meshes will be generated, at least one for each texture.", MessageType.Info);
            }

            GUILayout.Space(10);
        }

        private static void TerrainProviderExtraFields()
        {
            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM || prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
            {
                prefs.ignoreSRTMErrors = EditorGUILayout.Toggle("Ignore SRTM errors", prefs.ignoreSRTMErrors);
            }

            if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps) BingMapsElevationExtraFields();
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox) MapboxElevationExtraFields();
            else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30) SRTM30ExtraFields();
        }

#endregion

#region Texture

        public static void InitTextureProviders()
        {
            providersTitle = RealWorldTerrainTextureProviderManager.GetProvidersTitle();
            providers = RealWorldTerrainTextureProviderManager.GetProviders();

            if (prefs.mapType == null) prefs.mapType = RealWorldTerrainTextureProviderManager.FindMapType(prefs.mapTypeID);
            providerIndex = prefs.mapType.provider.index;
        }

        private static void PrecalculateMaxLevel()
        {
            int tx = prefs.textureSize.x * prefs.terrainCount.x / 256;
            int ty = prefs.textureSize.y * prefs.terrainCount.y / 256;
            int textureLevel = 0;

            for (int z = 5; z < 24; z++)
            {
                double stx, sty, etx, ety;
                RealWorldTerrainUtils.LatLongToTile(prefs.leftLongitude, prefs.topLatitude, z, out stx, out sty);
                RealWorldTerrainUtils.LatLongToTile(prefs.rightLongitude, prefs.bottomLatitude, z, out etx, out ety);

                if (etx < stx) etx += 1 << z;

                if (etx - stx > tx && ety - sty > ty)
                {
                    textureLevel = z;
                    break;
                }
            }

            if (textureLevel == 0) textureLevel = 24;

            EditorGUILayout.HelpBox("Texture level = " + textureLevel + " will be used.", MessageType.Info);
        }

        private static void TextureGUI()
        {
            TextureProviderGUI();

            if (prefs.mapType.provider.types.Length > 1)
            {
                GUIContent[] availableTypes = prefs.mapType.provider.types.Select(t => new GUIContent(t.title)).ToArray();
                int mti = prefs.mapType.index;
                EditorGUI.BeginChangeCheck();
                mti = EditorGUILayout.Popup(new GUIContent("Type", "Type of map texture"), mti, availableTypes);
                if (EditorGUI.EndChangeCheck())
                {
                    prefs.mapType = prefs.mapType.provider.types[mti];
                    prefs.mapTypeID = prefs.mapType.fullID;
                }
            }

            TextureProviderExtraFields();
            TextureProviderHelp();

            if (prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Count textures.    X");
                prefs.textureCount.x = Mathf.Max(EditorGUILayout.IntField(prefs.textureCount.x), 1);
                GUILayout.Label("Y");
                prefs.textureCount.y = Mathf.Max(EditorGUILayout.IntField(prefs.textureCount.y), 1);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            prefs.textureSize.x = EditorGUILayout.IntPopup("Texture width", prefs.textureSize.x, labelsTextureSize, valuesTextureSize);
            prefs.textureSize.y = EditorGUILayout.IntPopup("height", prefs.textureSize.y, labelsTextureSize, valuesTextureSize);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            prefs.textureFileType = (RealWorldTerrainTextureFileType)EditorGUILayout.EnumPopup("Format", prefs.textureFileType);
            if (prefs.textureFileType == RealWorldTerrainTextureFileType.jpg) prefs.textureFileQuality = EditorGUILayout.IntSlider("Quality", prefs.textureFileQuality, 0, 100);
            EditorGUILayout.EndHorizontal();

            List<string> levels = new List<string> { "Auto" };
            for (int i = 5; i < 25; i++) levels.Add(i.ToString());
            int index = prefs.maxTextureLevel;
            if (index != 0) index -= 4;
            index = EditorGUILayout.Popup("Max level", index, levels.ToArray());
            prefs.maxTextureLevel = index;
            if (index != 0) prefs.maxTextureLevel += 4;
            else
            {
                PrecalculateMaxLevel();
                prefs.reduceTextures = Toggle(prefs.reduceTextures, "Reduce size of textures, with no levels of tiles?",
                    "Reducing the size of the texture, reduces the time texture generation and memory usage.");
            }
            EditorGUILayout.Space();
        }

        private static void TextureProviderExtraFields()
        {
            RealWorldTerrainTextureProviderManager.IExtraField[] extraFields = prefs.mapType.extraFields;
            if (extraFields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in extraFields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }

            extraFields = prefs.mapType.provider.extraFields;
            if (extraFields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in extraFields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }
        }

        private static void TextureProviderExtraField(RealWorldTerrainTextureProviderManager.ExtraField field)
        {
            field.value = EditorGUILayout.TextField(field.title, field.value);
        }

        private static void TextureProviderGUI()
        {
            EditorGUI.BeginChangeCheck();
            providerIndex = EditorGUILayout.Popup("Provider", providerIndex, providersTitle);
            if (EditorGUI.EndChangeCheck())
            {
                prefs.mapType = providers[providerIndex].types[0];
                prefs.mapTypeID = prefs.mapType.fullID;
            }

            if (prefs.mapType.isCustom)
            {
                prefs.textureProviderURL = EditorGUILayout.TextField(prefs.textureProviderURL);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                showCustomProviderTokens = Foldout(showCustomProviderTokens, "Available tokens");
                if (showCustomProviderTokens)
                {
                    GUILayout.Label("{zoom}");
                    GUILayout.Label("{x}");
                    GUILayout.Label("{y}");
                    GUILayout.Label("{quad}");
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static void TextureProviderHelp()
        {
            string[] help = prefs.mapType.help;
            if (help != null)
            {
                foreach (string field in help)
                {
                    EditorGUILayout.HelpBox(field, MessageType.Info);
                }
            }

            help = prefs.mapType.provider.help;
            if (help != null)
            {
                foreach (string field in help)
                {
                    EditorGUILayout.HelpBox(field, MessageType.Info);
                }
            }
        }

        private static void TextureProviderToggleExtraGroup(RealWorldTerrainTextureProviderManager.ToggleExtraGroup @group)
        {
            @group.value = EditorGUILayout.Toggle(@group.title, @group.value);
            EditorGUI.BeginDisabledGroup(@group.value);

            if (@group.fields != null)
            {
                foreach (RealWorldTerrainTextureProviderManager.IExtraField field in @group.fields)
                {
                    if (field is RealWorldTerrainTextureProviderManager.ToggleExtraGroup) TextureProviderToggleExtraGroup(field as RealWorldTerrainTextureProviderManager.ToggleExtraGroup);
                    else if (field is RealWorldTerrainTextureProviderManager.ExtraField) TextureProviderExtraField(field as RealWorldTerrainTextureProviderManager.ExtraField);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

#endregion

#region Other

        private static void OSMGUI()
        {
            EditorGUIUtility.labelWidth = LabelWidth + 20;

            if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.additional)
            {
                prefs.elevationType = (RealWorldTerrainElevationType) EditorGUILayout.EnumPopup("Elevation", prefs.elevationType);
            }
            else prefs.elevationType = RealWorldTerrainElevationType.realWorld;
            

            OSMBuildings();
            OSMRoads();
            OSMRivers();

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                OSMTrees();
                OSMGrass();
            }
            else
            {
                prefs.generateGrass = false;
                prefs.generateTrees = false;
            }

            EditorGUIUtility.labelWidth = LabelWidth;
        }

        private static void OSMBuildings()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateBuildings = EditorGUILayout.Toggle("Generate buildings", prefs.generateBuildings);

            if (prefs.generateBuildings)
            {
                List<string> availableBuilders = new List<string> { "Built-in", "Instantiate Prefabs" };
                List<int> availableBuildersID = new List<int> { 0, 3 };
#if BUILDR2
                availableBuilders.Add("BuildR2");
                availableBuildersID.Add(2);
#endif
#if PROCEDURAL_TOOLKIT
                availableBuilders.Add("Procedural Toolkit");
                availableBuildersID.Add(4);
#endif

                prefs.buildingGenerator = EditorGUILayout.IntPopup("Building generator", prefs.buildingGenerator, availableBuilders.ToArray(), availableBuildersID.ToArray());

                prefs.buildingSingleRequest = EditorGUILayout.Toggle("Download using a single request", prefs.buildingSingleRequest);

                if (prefs.buildingGenerator == 2)
                {
#if BUILDR2
                    prefs.buildRCollider = (RealWorldTerrainBuildR2Collider)EditorGUILayout.EnumPopup("Collider", prefs.buildRCollider);
                    prefs.buildRRenderMode = (RealWorldTerrainBuildR2RenderMode)EditorGUILayout.EnumPopup("Render Mode", prefs.buildRRenderMode);

                    if (prefs.buildR2Materials == null) prefs.buildR2Materials = new List<RealWorldTerrainBuildR2Material>{new RealWorldTerrainBuildR2Material()};
                    else if (prefs.buildR2Materials.Count == 0)
                    {
                        prefs.buildR2Materials.Add(new RealWorldTerrainBuildR2Material());
                    }

                    EditorGUILayout.LabelField("Surfaces & Facades");

                    int removeIndex = -1;

                    for (int i = 0; i < prefs.buildR2Materials.Count; i++)
                    {
                        RealWorldTerrainBuildR2Material material = prefs.buildR2Materials[i];
                        EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenStyle : oddStyle);

                        EditorGUILayout.LabelField(i + 1 + ": ", GUILayout.Width(30));
                        EditorGUILayout.BeginVertical();

                        material.roofSurface = EditorGUILayout.ObjectField(new GUIContent("Roof Surface:"), material.roofSurface, typeof(Surface), false) as Surface;
                        material.roofType = (Roof.Types)EditorGUILayout.EnumPopup("Roof Type:", material.roofType);

                        if (material.facades == null) material.facades = new List<Facade>();

                        EditorGUILayout.LabelField("Facades:");
                        int removeFacadeIndex = -1;

                        for (int j = 0; j < material.facades.Count; j++)
                        {
                            EditorGUI.BeginChangeCheck();
                            material.facades[j] = EditorGUILayout.ObjectField(material.facades[j], typeof(Facade), false) as Facade;
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (material.facades[j] == null) removeFacadeIndex = j;
                            }
                        }

                        if (removeFacadeIndex != -1) material.facades.RemoveAt(removeFacadeIndex);

                        Facade newFacade = EditorGUILayout.ObjectField(null, typeof(Facade), false) as Facade;
                        if (newFacade != null) material.facades.Add(newFacade);

                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;

                        EditorGUILayout.EndHorizontal();
                    }

                    if (removeIndex != -1) prefs.buildR2Materials.RemoveAt(removeIndex);
                    if (GUILayout.Button("Add new item"))
                    {
                        prefs.buildR2Materials.Add(new RealWorldTerrainBuildR2Material());
                    }
#endif
                }
                else if (prefs.buildingGenerator == 3)
                {
                    EditorGUILayout.HelpBox("If you use highly detailed buildings, when generating areas that contain a lot of buildings (for example, cities), Unity Editor can crashes.", MessageType.Info);
                    EditorGUILayout.HelpBox("Prefab must contain a BoxCollider so that RWT can determine the boundaries of the building.", MessageType.Info);

                    if (prefs.buildingPrefabs == null) prefs.buildingPrefabs = new List<RealWorldTerrainBuildingPrefab>();

                    int removeIndex = -1;

                    for (int i = 0; i < prefs.buildingPrefabs.Count; i++)
                    {
                        RealWorldTerrainBuildingPrefab b = prefs.buildingPrefabs[i];
                        EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenStyle : oddStyle);

                        EditorGUILayout.LabelField(i + 1 + ": ", GUILayout.Width(30));
                        EditorGUILayout.BeginVertical();

                        b.prefab = EditorGUILayout.ObjectField("Prefab", b.prefab, typeof(GameObject), false) as GameObject;
                        b.sizeMode = (RealWorldTerrainBuildingPrefab.SizeMode) EditorGUILayout.EnumPopup("Size Mode", b.sizeMode);
                        b.heightMode = (RealWorldTerrainBuildingPrefab.HeightMode) EditorGUILayout.EnumPopup("Height Mode", b.heightMode);

                        if (b.heightMode == RealWorldTerrainBuildingPrefab.HeightMode.fixedHeight)
                        {
                            b.fixedHeight = EditorGUILayout.FloatField("Height", b.fixedHeight);
                        }

                        if (b.tags == null) b.tags = new List<RealWorldTerrainBuildingPrefab.OSMTag>();

                        EditorGUILayout.LabelField("Tags (if no tags, prefab can be used for all buildings)");

                        int tagRemoveIndex = -1;

                        for (int j = 0; j < b.tags.Count; j++)
                        {
                            EditorGUILayout.BeginHorizontal(j % 2 == 0 ? evenStyle : oddStyle);

                            EditorGUILayout.LabelField(j + 1 + ": ", GUILayout.Width(30));
                            EditorGUILayout.BeginVertical();

                            RealWorldTerrainBuildingPrefab.OSMTag t = b.tags[j];
                            t.key = EditorGUILayout.TextField("Key", t.key);
                            t.value = EditorGUILayout.TextField("Value", t.value);

                            EditorGUILayout.EndVertical();

                            if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) tagRemoveIndex = j;

                            EditorGUILayout.EndHorizontal();
                        }

                        if (tagRemoveIndex != -1) b.tags.RemoveAt(tagRemoveIndex);
                        if (GUILayout.Button("Add tag"))
                        {
                            b.tags.Add(new RealWorldTerrainBuildingPrefab.OSMTag());
                        }

                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;

                        EditorGUILayout.EndHorizontal();
                    }

                    if (removeIndex != -1) prefs.buildingPrefabs.RemoveAt(removeIndex);
                    if (GUILayout.Button("Add new item"))
                    {
                        RealWorldTerrainBuildingPrefab newPrefab = new RealWorldTerrainBuildingPrefab();
                        newPrefab.tags = new List<RealWorldTerrainBuildingPrefab.OSMTag>();
                        prefs.buildingPrefabs.Add(newPrefab);
                    }
                }
                else if (prefs.buildingGenerator == 4)
                {
#if PROCEDURAL_TOOLKIT
                    prefs.ptFacadePlanningStrategy = EditorGUILayout.ObjectField("Facade Planning Strategy", prefs.ptFacadePlanningStrategy, typeof(FacadePlanningStrategy), false) as FacadePlanningStrategy;
                    prefs.ptFacadeConstructionStrategy = EditorGUILayout.ObjectField("Facade Construction Strategy", prefs.ptFacadeConstructionStrategy, typeof(FacadeConstructionStrategy), false) as FacadeConstructionStrategy;
                    prefs.ptRoofPlanningStrategy = EditorGUILayout.ObjectField("Roof Planning Strategy", prefs.ptRoofPlanningStrategy, typeof(RoofPlanningStrategy), false) as RoofPlanningStrategy;
                    prefs.ptRoofConstructionStrategy = EditorGUILayout.ObjectField("Roof Construction Strategy", prefs.ptRoofConstructionStrategy, typeof(RoofConstructionStrategy), false) as RoofConstructionStrategy;
#endif
                }

                RealWorldTerrainRangeI range = prefs.buildingFloorLimits;
                float minLevelLimit = range.min;
                float maxLevelLimit = range.max;
                EditorGUILayout.MinMaxSlider(
                    new GUIContent(string.Format("Levels if unknown ({0}-{1})", range.min, range.max)), ref minLevelLimit,
                    ref maxLevelLimit, 1, 50);
                range.Set(minLevelLimit, maxLevelLimit);

                if (prefs.buildingGenerator == 0)
                {
                    prefs.buildingFloorHeight = EditorGUILayout.FloatField("Floor Height", prefs.buildingFloorHeight);
                    prefs.buildingUseColorTags = EditorGUILayout.Toggle("Use Color Tags", prefs.buildingUseColorTags);
                    prefs.buildingSaveInResult = EditorGUILayout.Toggle("Save In Result", prefs.buildingSaveInResult);
                    GUILayout.Label("Building Materials");
                    if (prefs.buildingMaterials == null) prefs.buildingMaterials = new List<RealWorldTerrainBuildingMaterial>();
                    for (int i = 0; i < prefs.buildingMaterials.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        GUILayout.Label((i + 1).ToString(), GUILayout.ExpandWidth(false));

                        EditorGUILayout.BeginVertical();

                        RealWorldTerrainBuildingMaterial material = prefs.buildingMaterials[i];
                        material.wall = EditorGUILayout.ObjectField("Wall material", material.wall, typeof(Material), false) as Material;
                        material.roof = EditorGUILayout.ObjectField("Roof material", material.roof, typeof(Material), false) as Material;

                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                        {
                            prefs.buildingMaterials[i] = null;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    prefs.buildingMaterials.RemoveAll(m => m == null);

                    if (GUILayout.Button("Add material")) prefs.buildingMaterials.Add(new RealWorldTerrainBuildingMaterial());
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void OSMGrass()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateGrass = EditorGUILayout.Toggle("Generate grass", prefs.generateGrass);
            if (!prefs.generateGrass)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            List<string> availableGrassType = new List<string>();
            availableGrassType.Add("Standard");
#if VOLUMEGRASS
            availableGrassType.Add("Volume Grass");
#endif
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            availableGrassType.Add("Vegetation Studio");
#endif

            if (availableGrassType.Count > 1)
            {
                int grassEngineIndex = availableGrassType.IndexOf(prefs.grassEngine);
                if (grassEngineIndex == -1) grassEngineIndex = 0;
                grassEngineIndex = EditorGUILayout.Popup("Grass engine ", grassEngineIndex, availableGrassType.ToArray());
                prefs.grassEngine = availableGrassType[grassEngineIndex];
            }
            else prefs.grassEngine = availableGrassType[0];

            if (prefs.grassEngine == "Standard")
            {
                prefs.grassDensity = EditorGUILayout.IntField("Density (%)", prefs.grassDensity);
                if (prefs.grassDensity < 1) prefs.grassDensity = 1;
                if (prefs.grassPrefabs == null) prefs.grassPrefabs = new List<Texture2D>();

                EditorGUILayout.LabelField("Grass Prefabs");
                for (int i = 0; i < prefs.grassPrefabs.Count; i++)
                {
                    prefs.grassPrefabs[i] =
                        (Texture2D)
                        EditorGUILayout.ObjectField(i + 1 + ":", prefs.grassPrefabs[i], typeof(Texture2D), false);
                }
                Texture2D newGrass =
                    (Texture2D)
                    EditorGUILayout.ObjectField(prefs.grassPrefabs.Count + 1 + ":", null, typeof(Texture2D), false);
                if (newGrass != null) prefs.grassPrefabs.Add(newGrass);
                prefs.grassPrefabs.RemoveAll(go => go == null);
            }
#if VOLUMEGRASS
            else if (prefs.grassEngine == "Volume Grass")
            {
                EditorGUILayout.HelpBox("Important: points outside terrains can crash Unity Editor.", MessageType.Info);
                prefs.volumeGrassOutsidePoints = (RealWorldTerrainVolumeGrassOutsidePoints) EditorGUILayout.EnumPopup("Outside Points", prefs.volumeGrassOutsidePoints);
            }
#endif
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            else if (prefs.grassEngine == "Vegetation Studio")
            {
#if !VEGETATION_STUDIO_PRO
                prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationPackage), false) as AwesomeTechnologies.VegetationPackage;
#else
                prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationSystem.VegetationPackagePro), false) as AwesomeTechnologies.VegetationSystem.VegetationPackagePro;
#endif
                if (prefs.vegetationStudioGrassTypes == null) prefs.vegetationStudioGrassTypes = new List<int> {1};

                int removeIndex = -1;
                for (int i = 0; i < prefs.vegetationStudioGrassTypes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    prefs.vegetationStudioGrassTypes[i] = EditorGUILayout.IntSlider("Vegetation Type " + (i + 1) + ": ", prefs.vegetationStudioGrassTypes[i], 1, 32);
                    if (prefs.vegetationStudioGrassTypes.Count > 1 && GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                if (removeIndex != -1) prefs.vegetationStudioGrassTypes.RemoveAt(removeIndex);
                if (GUILayout.Button("Add item")) prefs.vegetationStudioGrassTypes.Add(1);
            }
#endif

                EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private static void OSMRivers()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateRivers = EditorGUILayout.Toggle("Generate rivers", prefs.generateRivers);
            if (!prefs.generateRivers)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            prefs.riverMaterial = EditorGUILayout.ObjectField("Material", prefs.riverMaterial, typeof(Material), false) as Material;

            EditorGUILayout.EndVertical();
        }

        private static void OSMRoads()
        {
            List<string> availableRoadType = new List<string>();
#if EASYROADS3D
            availableRoadType.Add("EasyRoads3D");
#endif
#if ROADARCHITECT
            availableRoadType.Add("Road Architect");
#endif

            if (availableRoadType.Count == 0)
            {
                prefs.generateRoads = false;
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateRoads = EditorGUILayout.Toggle("Generate roads", prefs.generateRoads);

            if (!prefs.generateRoads)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.HelpBox("If the selected area contains cities, generation of roads can take a VERY long time.", MessageType.Info);

            if (availableRoadType.Count == 1)
            {
                EditorGUILayout.LabelField("Road engine - " + availableRoadType[0]);
                prefs.roadEngine = availableRoadType[0];
            }
            else
            {
                int roadEngineIndex = availableRoadType.IndexOf(prefs.roadEngine);
                if (roadEngineIndex == -1) roadEngineIndex = 0;
                roadEngineIndex = EditorGUILayout.Popup("Road engine", roadEngineIndex, availableRoadType.ToArray());
                prefs.roadEngine = availableRoadType[roadEngineIndex];
            }

            if (prefs.roadEngine == "EasyRoads3D")
            {
                prefs.erSnapToTerrain = EditorGUILayout.Toggle("Snap to Terrain", prefs.erSnapToTerrain);

                if (Math.Abs(prefs.erWidthMultiplier - 1) > float.Epsilon && prefs.erGenerateConnection)
                {
                    EditorGUILayout.HelpBox("If you select any connection, the width of the road will be reset to the default value.", MessageType.Warning);
                }

                prefs.roadTypeMode = (RealWorldTerrainRoadTypeMode) EditorGUILayout.EnumPopup("Mode", prefs.roadTypeMode);
                if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.simple)
                {
                    prefs.erWidthMultiplier = EditorGUILayout.FloatField("Width Multiplier", prefs.erWidthMultiplier);
                }

                prefs.erGenerateConnection = EditorGUILayout.Toggle("Generate Connections", prefs.erGenerateConnection);
                if (prefs.erGenerateConnection)
                {
                    EditorGUILayout.HelpBox("Important: the ability to generate connections is in beta. \nThis means that some roads may be not connected. \nWe and AndaSoft are working to improve this feature.", MessageType.Warning);
                }
            }
            else prefs.roadTypeMode = RealWorldTerrainRoadTypeMode.simple;

            if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.simple)
            {
#if UNITY_2017_3_OR_NEWER
                prefs.roadTypes = (RealWorldTerrainRoadType) EditorGUILayout.EnumFlagsField("Road types", prefs.roadTypes);
#else
                prefs.roadTypes = (RealWorldTerrainRoadType)EditorGUILayout.EnumMaskField("Road types", prefs.roadTypes);
#endif
            }
            else if (prefs.roadTypeMode == RealWorldTerrainRoadTypeMode.advanced)
            {
#if EASYROADS3D
                if (roadNetwork == null && needFindRoadNetwork)
                {
                    roadNetwork = Object.FindObjectOfType<ERModularBase>();
                    needFindRoadNetwork = false;
                }

                if (roadNetwork == null)
                {
                    EditorGUILayout.HelpBox("Mode - Advanced requires a road network in the scene.", MessageType.Error);

                    if (GUILayout.Button("Find Road Network in scene"))
                    {
                        roadNetwork = Object.FindObjectOfType<ERModularBase>();
                    }

                    if (GUILayout.Button("Create Road Network"))
                    {
                        ERRoadNetwork newRoadNetwork = new ERRoadNetwork();
                        roadNetwork = newRoadNetwork.roadNetwork;
                    }
                }

                if (roadTypeNames == null) roadTypeNames = Enum.GetNames(typeof(RealWorldTerrainRoadType));

                if (roadNetwork == null)
                {
                    foreach (string name in roadTypeNames) EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(name));
                }
                else
                {
                    if (erRoadTypeNames == null) UpdateERRoadTypes();

                    if (GUILayout.Button("Update EasyRoad3D Road Types")) UpdateERRoadTypes();

                    if (erRoadTypeNames != null)
                    {
                        if (prefs.erRoadTypes == null) prefs.erRoadTypes = new string[roadTypeNames.Length];
                        if (prefs.erRoadTypes.Length != erRoadTypeNames.Length) Array.Resize(ref prefs.erRoadTypes, roadTypeNames.Length);

                        for (int i = 0; i < roadTypeNames.Length; i++)
                        {
                            int index = 0;
                            string roadTypeName = prefs.erRoadTypes[i];

                            if (!string.IsNullOrEmpty(roadTypeName))
                            {
                                for (int j = 0; j < erRoadTypeNames.Length; j++)
                                {
                                    if (erRoadTypeNames[j] == roadTypeName)
                                    {
                                        index = j;
                                        break;
                                    }
                                }
                            }
                            
                            index = EditorGUILayout.Popup(ObjectNames.NicifyVariableName(roadTypeNames[i]), index, erRoadTypeNames);
                            if (index != 0) prefs.erRoadTypes[i] = erRoadTypeNames[index];
                            else prefs.erRoadTypes[i] = string.Empty;
                        }
                    }
                }
#endif
            }

            EditorGUILayout.EndVertical();
        }

        private static void UpdateERRoadTypes()
        {
#if EASYROADS3D
            ERRoadType[] erRoadTypes = roadNetwork.GetRoadTypes();
            if (erRoadTypes != null) erRoadTypeNames = new []{"Ignore"}.Concat(erRoadTypes.Select(t => t.roadTypeName)).ToArray();
            else erRoadTypeNames = null;
#endif
        }

        private static void OSMTrees()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            prefs.generateTrees = EditorGUILayout.Toggle("Generate trees", prefs.generateTrees);

            if (!prefs.generateTrees)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            List<string> availableTreeType = new List<string>();
            availableTreeType.Add("Standard");
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            availableTreeType.Add("Vegetation Studio");
#endif

            if (availableTreeType.Count > 1)
            {
                int treeEngineIndex = availableTreeType.IndexOf(prefs.treeEngine);
                if (treeEngineIndex == -1) treeEngineIndex = 0;
                treeEngineIndex = EditorGUILayout.Popup("Tree engine ", treeEngineIndex, availableTreeType.ToArray());
                prefs.treeEngine = availableTreeType[treeEngineIndex];
            }
            else prefs.treeEngine = availableTreeType[0];

            if (prefs.treeEngine == "Standard")
            {
                prefs.treeDensity = EditorGUILayout.IntField("Density (%)", prefs.treeDensity);
                if (prefs.treeDensity < 1) prefs.treeDensity = 1;
                if (prefs.treePrefabs == null) prefs.treePrefabs = new List<GameObject>();
                EditorGUILayout.LabelField("Tree Prefabs");
                for (int i = 0; i < prefs.treePrefabs.Count; i++)
                {
                    prefs.treePrefabs[i] =
                        (GameObject)
                        EditorGUILayout.ObjectField(i + 1 + ":", prefs.treePrefabs[i], typeof(GameObject), false);
                }

                GameObject newTree =
                    (GameObject)
                    EditorGUILayout.ObjectField(prefs.treePrefabs.Count + 1 + ":", null, typeof(GameObject), false);
                if (newTree != null) prefs.treePrefabs.Add(newTree);
                prefs.treePrefabs.RemoveAll(go => go == null);
            }
            else if (prefs.treeEngine == "Vegetation Studio")
            {
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
#if !VEGETATION_STUDIO_PRO
                prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationPackage), false) as AwesomeTechnologies.VegetationPackage;
#else
                prefs.vegetationStudioPackage = EditorGUILayout.ObjectField("Package", prefs.vegetationStudioPackage, typeof(AwesomeTechnologies.VegetationSystem.VegetationPackagePro), false) as AwesomeTechnologies.VegetationSystem.VegetationPackagePro;
#endif
                if (prefs.vegetationStudioTreeTypes == null) prefs.vegetationStudioTreeTypes = new List<int>{1};

                int removeIndex = -1;
                for (int i = 0; i < prefs.vegetationStudioTreeTypes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    prefs.vegetationStudioTreeTypes[i] = EditorGUILayout.IntSlider("Vegetation Type " + (i + 1) + ": ", prefs.vegetationStudioTreeTypes[i], 1, 32);
                    if (prefs.vegetationStudioTreeTypes.Count > 1 && GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                if (removeIndex != -1) prefs.vegetationStudioTreeTypes.RemoveAt(removeIndex);
                if (GUILayout.Button("Add item")) prefs.vegetationStudioTreeTypes.Add(1);
#endif
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private static void POI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (prefs.POI == null) prefs.POI = new List<RealWorldTerrainPOI>();

            if (GUILayout.Button(new GUIContent("+", "Add POI"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                prefs.POI.Add(new RealWorldTerrainPOI("New POI " + (prefs.POI.Count + 1),
                    (prefs.leftLongitude + prefs.rightLongitude) / 2, (prefs.topLatitude + prefs.bottomLatitude) / 2));
            }

            GUILayout.Label("");

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) prefs.POI = new List<RealWorldTerrainPOI>();

            EditorGUILayout.EndHorizontal();

            if (prefs.POI.Count == 0)
            {
                GUILayout.Label("POI is not specified.");
            }

            int poiIndex = 1;
            int poiDeleteIndex = -1;

            foreach (RealWorldTerrainPOI poi in prefs.POI)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(poiIndex.ToString(), GUILayout.ExpandWidth(false));
                poi.title = EditorGUILayout.TextField("", poi.title);
                GUILayout.Label("Lat", GUILayout.ExpandWidth(false));
                poi.y = EditorGUILayout.DoubleField("", poi.y, GUILayout.Width(80));
                GUILayout.Label("Lng", GUILayout.ExpandWidth(false));
                poi.x = EditorGUILayout.DoubleField("", poi.x, GUILayout.Width(80));
                if (GUILayout.Button(new GUIContent("X", "Delete POI"), GUILayout.ExpandWidth(false))) poiDeleteIndex = poiIndex - 1;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Altitude", GUILayout.ExpandWidth(false));
                poi.altitude = EditorGUILayout.FloatField("", poi.altitude, GUILayout.Width(40));
                GUILayout.Label("Prefab", GUILayout.ExpandWidth(false));
                poi.prefab = EditorGUILayout.ObjectField("", poi.prefab, typeof(GameObject), false) as GameObject;
                EditorGUILayout.EndHorizontal();

                poiIndex++;
            }

            if (poiDeleteIndex != -1) prefs.POI.RemoveAt(poiDeleteIndex);

            EditorGUILayout.Space();
        }

#endregion

#endregion

#region Generate

        private static void OnGenerate()
        {
            if (phase != null && phase is RealWorldTerrainDownloadingPhase)
            {
                int completed = Mathf.FloorToInt(RealWorldTerrainDownloadManager.totalSizeMB * RealWorldTerrainWindow.progress);
                GUILayout.Label(phasetitle + " (" + completed + " of " + RealWorldTerrainDownloadManager.totalSizeMB + " mb)");
            }
            else GUILayout.Label(phasetitle);

            Rect r = EditorGUILayout.BeginVertical();
            iProgress = Mathf.FloorToInt(RealWorldTerrainWindow.progress * 100);
            EditorGUI.ProgressBar(r, RealWorldTerrainWindow.progress, iProgress + "%");
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Cancel")) RealWorldTerrainWindow.CancelCapture();

            GUILayout.Label("Warning: Keep this window open.");
        }

#endregion

#endregion
    }
}