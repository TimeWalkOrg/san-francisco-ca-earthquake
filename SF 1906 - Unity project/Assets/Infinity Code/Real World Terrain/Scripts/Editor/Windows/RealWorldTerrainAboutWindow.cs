/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainAboutWindow : EditorWindow
    {
        [MenuItem("Window/Infinity Code/Real World Terrain/About", false, 2000)]
        public static void OpenWindow()
        {
            RealWorldTerrainAboutWindow window = GetWindow<RealWorldTerrainAboutWindow>(true, "About", true);
            window.minSize = new Vector2(200, 100);
            window.maxSize = new Vector2(200, 100);
        }

        public void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("Real World Terrain", titleStyle);
            GUILayout.Label("version " + RealWorldTerrainWindow.version, textStyle);
            GUILayout.Label("created Infinity Code", textStyle);
            GUILayout.Label("2013-" + DateTime.Now.Year, textStyle);
        }
    }
}
