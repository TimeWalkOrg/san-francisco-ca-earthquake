using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WireBuilder
{
    public class HelpWindow : EditorWindow
    {
        [MenuItem("Help/Wire Builder", false, 0)]
        public static void ExecuteMenuItem()
        {
            HelpWindow.ShowWindow();
        }

        //Window properties
        private static int width = 440;
        private static int height = 350;

        private bool isTabInstallation = true;
        private bool isTabDocumentation;
        private bool isTabSupport;
        private Color defaultColor;

        private static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow<HelpWindow>(false, " Help", true);
            editorWindow.titleContent = new GUIContent(" Help", WireBuilderGUI.GroupIcon.image);

            //Open somewhat in the center of the screen
            editorWindow.position = new Rect((Screen.width) / 2f + width, (Screen.height) / 2f, (width * 2), height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, 200);

            WireBuilderUtilities.GetRenderPipeline();
            AssetInfo.GetRootFolder();
            AssetInfo.VersionChecking.CheckUnityVersion();
            AssetInfo.VersionChecking.CheckForUpdate(false);

            editorWindow.Show();
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(5);
            DrawTabs();

            EditorGUILayout.BeginVertical(WireBuilderGUI.ParameterGroup.Section);

            if (isTabInstallation) DrawInstallation();

            if (isTabDocumentation) DrawDocumentation();

            if (isTabSupport) DrawSupport();

            EditorGUILayout.EndVertical();

            WireBuilderGUI.DrawFooter();
        }

        private void DrawInstallation()
        {
            //Version

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Package"));

            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent(" Package version", EditorGUIUtility.IconContent("cs Script Icon").image));

                defaultColor = GUI.contentColor;
                if (AssetInfo.IS_UPDATED)
                {

                    GUI.contentColor = Color.green;
                    EditorGUILayout.LabelField("Up-to-date");
                    GUI.contentColor = defaultColor;

                }
                else
                {
                    GUILayout.FlexibleSpace();

                    GUI.contentColor = new Color(1f, 0.65f, 0f);
                    EditorGUILayout.LabelField("Outdated", EditorStyles.boldLabel, GUILayout.MaxWidth(75f));
                    GUI.contentColor = defaultColor;
                    if (GUILayout.Button(new GUIContent("Update package"), EditorStyles.miniButton))
                    {
                        AssetInfo.OpenStorePage();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent(" Unity version", EditorGUIUtility.IconContent("UnityLogo").image));

                if (AssetInfo.compatibleVersion)
                {
                    GUI.contentColor = Color.green;
                    EditorGUILayout.LabelField("Compatible");
                    GUI.contentColor = defaultColor;
                }
                else if (AssetInfo.untestedVersion)
                {
                    GUI.contentColor = new Color(1f, 0.65f, 0f);
                    EditorGUILayout.LabelField("Untested", EditorStyles.boldLabel);
                    GUI.contentColor = defaultColor;
                }
                EditorGUILayout.EndHorizontal();

                if (AssetInfo.compatibleVersion == false && AssetInfo.untestedVersion == false)
                {
                    GUI.contentColor = Color.red;
                    EditorGUILayout.LabelField("This version of Unity is not supported.", EditorStyles.boldLabel);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Please upgrade to at least Unity " + AssetInfo.MIN_UNITY_VERSION);
                }

                if (AssetInfo.untestedVersion)
                {
                    EditorGUILayout.LabelField("The current Unity version has not been tested yet, or compatibility is being worked on. You may run into issues.", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }

            }

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Render pipeline (" + WireBuilderUtilities.CurrentPipeline + ")"));

            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.HighDefinition)
                {
                    EditorGUILayout.HelpBox("You are using the High Definition Render Pipeline, the package does not include a compatible (animated) wire shader", MessageType.Warning);
                }
                using (new EditorGUI.DisabledGroupScope(WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.HighDefinition))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Setup");

                        bool hasError = false;

                        if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.HighDefinition) hasError = true;
#if !UNITY_2018_3_OR_NEWER //LWRP 3.0.0
                        if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.Lightweight) hasError = true;
#endif
                        using (new EditorGUI.DisabledGroupScope(hasError))
                        {
                            if (GUILayout.Button(new GUIContent("Configure materials"), EditorStyles.miniButton))
                            {
                                if (EditorUtility.DisplayDialog("Setup materials", "This will configure all materials in the Wire Builder package for the " + WireBuilderUtilities.CurrentPipeline + " render pipeline", "OK", "Cancel"))
                                {
                                    WireBuilderUtilities.SetupMaterials();
                                }
                            }
                        }
                    }
#if !UNITY_2018_3_OR_NEWER //LWRP 3.0.0
                    if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.Lightweight)
                    {
                        EditorGUILayout.HelpBox("Lightweight Render Pipeline 3.0.0 is not supported. Requires update to Unity 2018.3 or newer.", MessageType.Error);
                    }
#endif
                    if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.HighDefinition)
                    {
                        EditorGUILayout.HelpBox("No compatibility with the High Definition Render pipeline is available", MessageType.Error);
                    }

                }


            }
        }

        private void DrawDocumentation()
        {
            EditorGUILayout.HelpBox("Please view the documentation for further details about this package and its workings.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Documentation</size></b>\n<i>Usage instructions</i>", WireBuilderGUI.Button))
                {
                    Application.OpenURL(AssetInfo.DOC_URL);
                }
                if (GUILayout.Button("<b><size=12>Troubleshooting</size></b>\n<i>Known issues</i>", WireBuilderGUI.Button))
                {
                    Application.OpenURL(AssetInfo.DOC_URL + "?section=troubleshooting-8");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSupport()
        {
            EditorGUILayout.HelpBox("\nIf you have any questions, or ran into issues, please get in touch.\n", MessageType.Info);

            EditorGUILayout.Space();

            //Buttons box
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("<b><size=12>Email</size></b>\n<i>Contact</i>", WireBuilderGUI.Button))
                {
                    Application.OpenURL("mailto:contact@staggart.xyz");
                }
                if (GUILayout.Button("<b><size=12>Twitter</size></b>\n<i>Follow developments</i>", WireBuilderGUI.Button))
                {
                    Application.OpenURL("https://twitter.com/search?q=staggart%20creations");
                }
                if (GUILayout.Button("<b><size=12>Forum</size></b>\n<i>Join the discussion</i>", WireBuilderGUI.Button))
                {
                    Application.OpenURL(AssetInfo.FORUM_URL);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("<size=12> Rate</size>", EditorGUIUtility.IconContent("d_Favorite").image), WireBuilderGUI.Button)) AssetInfo.OpenStorePage();

            if (GUILayout.Button(new GUIContent("<size=12> Review</size>", EditorGUIUtility.IconContent("d_FilterByLabel").image), WireBuilderGUI.Button)) AssetInfo.OpenStorePage();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();


            if (GUILayout.Toggle(isTabInstallation, new GUIContent("Installation"), WireBuilderGUI.Tab))
            {
                isTabInstallation = true;
                isTabDocumentation = false;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabDocumentation, "Documentation", WireBuilderGUI.Tab))
            {
                isTabInstallation = false;
                isTabDocumentation = true;
                isTabSupport = false;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Toggle(isTabSupport, "Support", WireBuilderGUI.Tab))
            {
                isTabInstallation = false;
                isTabDocumentation = false;
                isTabSupport = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("<size=24>" + AssetInfo.ASSET_NAME + "</size>"), WireBuilderGUI.Header);

            GUILayout.Label("Version: " + AssetInfo.INSTALLED_VERSION, EditorStyles.centeredGreyMiniLabel);
        }
    }
}