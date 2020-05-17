/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Tools;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainItem))]
    public class RealWorldTerrainItemEditor : Editor
    {
        private RealWorldTerrainItem item;
        private Texture2D texture;

        public static void DrawLocationInfo(RealWorldTerrainMonoBase item)
        {
            GUILayout.Label("Top-Left: ");
            GUILayout.Label("  Latitude: " + item.topLatitude);
            GUILayout.Label("  Longitude: " + item.leftLongitude);
            EditorGUILayout.Space();
            GUILayout.Label("Bottom-Right: ");
            GUILayout.Label("  Latitude: " + item.bottomLatitude);
            GUILayout.Label("  Longitude: " + item.rightLongitude);
            EditorGUILayout.Space();
        }

        private void DrawToolbar()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbar)
            {
                fixedHeight = 40,
                padding = new RectOffset(5, 5, 4, 4)
            };
            EditorGUILayout.BeginHorizontal(style);

            GUIStyle buttonStyle = new GUIStyle { margin = new RectOffset(5, 5, 0, 0) };

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.refreshIcon, "Real World Terrain"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRegenerateMenu();
            if (item.generateTextures && GUILayout.Button(new GUIContent(RealWorldTerrainResources.wizardIcon, "Postprocess"), buttonStyle, GUILayout.ExpandWidth(false))) ShowPostprocessMenu();
            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.rawIcon, "Export/Import RAW"), buttonStyle, GUILayout.ExpandWidth(false))) ShowRawMenu();

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void OnEnable()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            item = (RealWorldTerrainItem)target;
        }

        public override void OnInspectorGUI()
        {
            DrawToolbar();
            DrawLocationInfo(item);

            Texture2D currentTexture = item.texture;
            EditorGUI.BeginChangeCheck();
            currentTexture = (Texture2D)EditorGUILayout.ObjectField("Texture: ", currentTexture, typeof(Texture2D), true);
            if (EditorGUI.EndChangeCheck()) item.texture = currentTexture;
        }

        private void OnUpdate()
        {
            if (item != null && item.needUpdate)
            {
                item.needUpdate = false;
                Repaint();
            }
        }

        private void ShowPostprocessMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Brightness, Contrast and HUE"), false, () => RealWorldTerrainHUEWindow.OpenWindow(item));
            menu.AddItem(new GUIContent("Color Balance"), false, () => RealWorldTerrainColorBalance.OpenWindow(item));
            menu.AddItem(new GUIContent("Color Levels"), false, () => RealWorldTerrainColorLevels.OpenWindow(item));

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Erosion"), false, () => RealWorldTerrainErosionFilter.OpenWindow(item));
                menu.AddItem(new GUIContent("Generate Grass from Texture"), false, () => RealWorldTerrainGrassGeneratorWindow.OpenWindow(item));
                menu.AddItem(new GUIContent("Generate SplatPrototypes from Texture"), false, () => RealWorldTerrainSplatPrototypeGenerator.OpenWindow(item));
            }

            menu.ShowAsContext();
        }

        private void ShowRawMenu()
        {
            GenericMenu menu = new GenericMenu();

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                menu.AddItem(new GUIContent("Export Heightmap"), false, () => RealWorldTerraiHeightmapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Heightmap"), false, () => RealWorldTerrainHeightmapImporter.OpenWindow(item));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Detailmap"), false, () => RealWorldTerrainDetailmapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Detailmap"), false, () => RealWorldTerrainDetailmapImporter.OpenWindow(item));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Alphamap"), false, () => RealWorldTerrainAlphamapExporter.OpenWindow(item));
                menu.AddItem(new GUIContent("Import Alphamap"), false, () => RealWorldTerrainAlphamapImporter.OpenWindow(item));
                
            }
            else if (item.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                menu.AddItem(new GUIContent("Export OBJ"), false, () => RealWorldTerrainMeshOBJExporter.Export(item));
            }

            if (item.generateTextures)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Export Textures"), false, () => RealWorldTerrainContainerEditor.ExportRawTextures(item));
                menu.AddItem(new GUIContent("Import Textures"), false, () => RealWorldTerrainContainerEditor.ImportRawTextures(item));
            }
            menu.ShowAsContext();
        }

        private void ShowRegenerateMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Regenerate Terrains"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.terrain, item));
            menu.AddItem(new GUIContent("Regenerate Textures"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.texture, item));
            menu.AddItem(new GUIContent("Regenerate Additional"), false, () => RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.additional, item));
            menu.ShowAsContext();
        }
    }
}