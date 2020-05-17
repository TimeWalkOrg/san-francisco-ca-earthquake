/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainMemoryUsageWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        private float mb = 1048576;

        private long heightmap;
        private long controltexture;
        private long detailmap;
        private int countGrass;
        private long basemap;
        private long texture;
        private long totalPerTerrain;
        private RealWorldTerrainVector2i countTerrains;
        private long total;

        private string heightmapS;


        private string controltextureS;
        private string detailmapS;
        private string basemapS;
        private string textureS;
        private string totalPerTerrainS;
        private string totalS;
        private string countTerrainsS;

        private void CalculateUsage()
        {
            RealWorldTerrainPrefs p = RealWorldTerrainWindow.prefs;
            if (p == null)
            {
                EditorUtility.DisplayDialog("Error", "Can not find the prefs. Open Real World Terrain window.", "OK");
                Close();
                return;
            }

            heightmap = p.heightmapResolution * p.heightmapResolution * 4;
            controltexture = p.controlTextureResolution * p.controlTextureResolution * 4;
            detailmap = p.detailResolution * p.detailResolution * 4;
            countGrass = p.generateGrass ? p.grassPrefabs.Count : 0;
            basemap = p.baseMapResolution * p.baseMapResolution * 4;
            texture = p.generateTextures ? p.textureSize.x * p.textureSize.y * 4: 0;
            totalPerTerrain = heightmap + countTerrains + detailmap * countGrass + basemap + texture;
            countTerrains = p.terrainCount;
            total = totalPerTerrain * countTerrains.count;

            string format = "{0:### ##0.00}";
            heightmapS = String.Format(format, heightmap / mb) + " mb";
            controltextureS = String.Format(format, controltexture / mb) + " mb";
            detailmapS = String.Format(format, detailmap * countGrass / mb) + " mb";
            basemapS = String.Format(format, basemap / mb) + " mb";
            textureS = String.Format(format, texture / mb) + " mb";
            totalPerTerrainS = String.Format(format, totalPerTerrain / mb) + " mb";
            countTerrainsS = countTerrains.count + " (" + countTerrains.x + "x" + countTerrains.y + ")";
            totalS = String.Format(format, total / mb) + " mb";
        }

        private void OnEnable()
        {
            CalculateUsage();
        }

        private void DrawField(string prefix, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(prefix, GUILayout.MaxWidth(position.size.x / 2 - 10));
            EditorGUILayout.LabelField(value, GUILayout.MaxWidth(position.size.x / 2 - 10));
            EditorGUILayout.EndHorizontal();
        }

        public void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.HelpBox("Uncompressed size of the result by the fields.\nHere only the main fields affecting the size are shown.\nNote that the memory that RWT will use for generation is not shown here.", MessageType.Info);

            DrawField("Height Map:", heightmapS);
            DrawField("Control Texture:", controltextureS);
            DrawField("Detail Map: ", detailmapS);
            DrawField("Base Map: ", basemapS);
            DrawField("Textures: ", textureS);
            EditorGUILayout.Space();
            DrawField("Total Per Terrain: ", totalPerTerrainS);
            EditorGUILayout.LabelField("---");
            DrawField("Count Terrains: ", countTerrainsS);
            DrawField("Total: ", totalS);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh"))
            {
                CalculateUsage();
            }
        }

        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainMemoryUsageWindow>(true, "Memory Usage", true);
        }
    }
}