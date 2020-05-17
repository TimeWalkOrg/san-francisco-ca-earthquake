/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.Generators;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof (RealWorldTerrainBuilding))]
    public class RealWorldTerrainBuildingEditor : Editor
    {
        private RealWorldTerrainBuilding building;

        private void InvertRoofNormals()
        {
            building.invertRoof = !building.invertRoof;
            UpdateRoof();
        }

        private void InvertWallNormals()
        {
            building.invertWall = !building.invertWall;
            UpdateWall();
        }

        public void OnEnable()
        {
            building = (RealWorldTerrainBuilding)target;
        }

        public override void OnInspectorGUI()
        {
            building.baseHeight = EditorGUILayout.FloatField("Base Height (meters): ", building.baseHeight);

            building.roofType = (RealWorldTerrainRoofType)EditorGUILayout.EnumPopup("Roof type: ", building.roofType);
            if (building.roofType != RealWorldTerrainRoofType.flat) building.roofHeight = EditorGUILayout.FloatField("Roof Height (meters): ", building.roofHeight);

            if (GUILayout.Button("Invert wall normals") && building.wall != null) InvertWallNormals();
            if (GUILayout.Button("Invert roof normals") && building.roof != null) InvertRoofNormals();
            if (GUILayout.Button("Update")) UpdateBuilding();

            if (GUILayout.Button("Export mesh to OBJ"))
            {
                string path = EditorUtility.SaveFilePanel("Save building to OBJ", "", building.name + ".obj", "obj");
                if (path.Length != 0) RealWorldTerrainUtils.ExportMesh(path, building.wall, building.roof);
            }
        }

        private void UpdateBuilding()
        {
            if (building.wall != null) UpdateWall();
            if (building.roof != null) UpdateRoof();
        }

        private void UpdateRoof()
        {
            building.roof.mesh = building.roof.sharedMesh =
                RealWorldTerrainBuildingGenerator.CreateHouseRoofMesh(building.baseVerticles, building.container.scale,
                    building.baseHeight, building.roofHeight, building.roofType, building.name, building.invertRoof);
        }

        private void UpdateWall()
        {
            building.wall.mesh = building.wall.sharedMesh =
                RealWorldTerrainBuildingGenerator.CreateHouseWallMesh(building.baseVerticles, building.container.scale,
                    building.baseHeight, building.name, building.invertWall);
        }
    }
}