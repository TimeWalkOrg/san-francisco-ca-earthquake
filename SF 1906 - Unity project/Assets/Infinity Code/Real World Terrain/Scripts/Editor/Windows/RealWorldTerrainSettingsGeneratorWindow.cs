/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainSettingsGeneratorWindow : EditorWindow
    {
        private int heightmapQuality = 50;
        private int textureQuality = 50;
        private Vector2 scrollPosition;
        private bool generateGrass;
        private bool generateTexture = true;

        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainSettingsGeneratorWindow>(false, "Settings generator");
        }

        private void OnGUI()
        {
            RealWorldTerrainPrefs prefs = RealWorldTerrainWindow.prefs;

            heightmapQuality = EditorGUILayout.IntSlider("Heightmap quality:", heightmapQuality, 1, 100);
            generateTexture = EditorGUILayout.Toggle("Generate texture:", generateTexture);
            if (generateTexture) textureQuality = EditorGUILayout.IntSlider("Texture quality:", textureQuality, 1, 100);
            generateGrass = EditorGUILayout.Toggle("Generate grass", generateGrass);

            double rangeX = prefs.rightLongitude - prefs.leftLongitude;
            double rangeY = prefs.topLatitude - prefs.bottomLatitude;

            double sizeX = 0;
            double sizeY = 0;

            if (prefs.sizeType == 0)
            {
                double scfY = Math.Sin(prefs.topLatitude * Mathf.Deg2Rad);
                double sctY = Math.Sin(prefs.bottomLatitude * Mathf.Deg2Rad);
                double ccfY = Math.Cos(prefs.topLatitude * Mathf.Deg2Rad);
                double cctY = Math.Cos(prefs.bottomLatitude * Mathf.Deg2Rad);
                double cX = Math.Cos(rangeX * Mathf.Deg2Rad);
                double sizeX1 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
                double sizeX2 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
                sizeX = (sizeX1 + sizeX2) / 2.0;
                sizeY = RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY);
            }
            else if (prefs.sizeType == 1)
            {
                sizeX = Math.Abs(rangeX / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
                sizeY = Math.Abs(rangeY / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
            }

            int hmX = (int)Math.Round(sizeX / 9 * heightmapQuality);
            int hmY = (int)Math.Round(sizeY / 9 * heightmapQuality);

            int tsX = generateTexture ? (int)Math.Round(sizeX * 10 * textureQuality) : 0;
            int tsY = generateTexture ? (int)Math.Round(sizeY * 10 * textureQuality) : 0;

            int countX = Mathf.Max(hmX / 4096 + 1, tsX / 4096 + 1);
            int countY = Mathf.Max(hmY / 4096 + 1, tsY / 4096 + 1);

            if (countX > 10 || countY > 10)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.red;
                style.fontStyle = FontStyle.Bold;
                style.wordWrap = true;
                GUILayout.Label("Too high a settings. Memory overflow may occur.", style);
            }

            int heightmapRes = Mathf.Max(hmX / countX, hmY / countY);
            heightmapRes = Mathf.Clamp(Mathf.NextPowerOfTwo(heightmapRes), 32, 4096);
            int detailRes = (generateGrass) ? heightmapRes : 32;
            int textureWidth = Mathf.Clamp(Mathf.NextPowerOfTwo(tsX / countX), 32, 4096);
            int textureHeight = Mathf.Clamp(Mathf.NextPowerOfTwo(tsY / countY), 32, 4096);
            int basemapRes = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.Max(textureWidth, textureHeight) / 4), 32, 4096);

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Area size X:{0} km, Y:{1} km", sizeX, sizeY));
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Recommended settings:");
            GUILayout.Label(string.Format("Count terrains: {0}x{1}", countX, countY));
            GUILayout.Label("Heightmap resolution: " + heightmapRes);
            GUILayout.Label("Detail resolution: " + detailRes);
            GUILayout.Label("Basemap resolution: " + basemapRes);
            if (generateTexture) GUILayout.Label(string.Format("Texture size: {0}x{1}", textureWidth, textureHeight));

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply"))
            {
                prefs.terrainCount = new RealWorldTerrainVector2i(countX, countY);
                prefs.heightmapResolution = heightmapRes;
                prefs.detailResolution = detailRes;
                prefs.baseMapResolution = basemapRes;

                if (generateTexture)
                {
                    prefs.textureSize = new RealWorldTerrainVector2i(textureWidth, textureHeight);
                    prefs.generateTextures = true;
                }
                else prefs.generateTextures = false;

                if (RealWorldTerrainWindow.wnd != null) RealWorldTerrainWindow.wnd.Repaint();

                Close();
            }
        }
    }
}