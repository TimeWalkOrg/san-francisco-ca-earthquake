/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.RealWorldTerrain.OSM;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainBuildingManager : EditorWindow
    {
        private IEnumerable<RealWorldTerrainOSMMeta> buildings;
        private Vector2 scrollPosition;
        private string filter;
        private IEnumerable<RealWorldTerrainOSMMeta> filteredBuildings;

        private void FilterBuildings()
        {
            if (string.IsNullOrEmpty(filter)) filteredBuildings = buildings;
            else
            {
                string f = filter.ToLower();
                filteredBuildings = buildings.Where(b => b.metaInfo.Any(i => i.title.ToLower().Contains(f) || i.info.ToLower().Contains(f)));
            }
        }

        private void OnEnable()
        {
            UpdateBuildings();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            filter = EditorGUILayout.TextField("Filter: ", filter);
            if (EditorGUI.EndChangeCheck()) FilterBuildings();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (RealWorldTerrainOSMMeta building in filteredBuildings)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(building.name);
                if (GUILayout.Button(new GUIContent(">", "Select"), GUILayout.ExpandWidth(false)))
                    Selection.activeGameObject = building.gameObject;

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Building Manager")]
        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainBuildingManager>(true, "Building Manager");
        }

        private void UpdateBuildings()
        {
            buildings = FindObjectsOfType<RealWorldTerrainBuilding>().Select(b => b.GetComponent<RealWorldTerrainOSMMeta>()).OrderBy(b => b.name);
            FilterBuildings();
        }
    }
}