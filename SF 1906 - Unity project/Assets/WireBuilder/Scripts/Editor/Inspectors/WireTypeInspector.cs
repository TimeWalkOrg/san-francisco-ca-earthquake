using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace WireBuilder
{
    [CustomEditor(typeof(WireType))]
    public class WireTypeInspector : Editor
    {

        SerializedProperty geometryType;
        SerializedProperty pointsPerMeter;
        SerializedProperty radialSegments;

        SerializedProperty weight;
        SerializedProperty diameter;

        SerializedProperty material;
        SerializedProperty textureMode;
        SerializedProperty tiling;

        SerializedProperty layer;
        SerializedProperty tag;

        SerializedProperty enableTreeMask;
        SerializedProperty enableLargeObjectMask;
        SerializedProperty treeMaskWidth;
        SerializedProperty largeObjectMaskWidth;

        private List<Wire> targetWires;

        private void OnEnable()
        {
            geometryType = serializedObject.FindProperty("geometryType");
            pointsPerMeter = serializedObject.FindProperty("pointsPerMeter");
            radialSegments = serializedObject.FindProperty("radialSegments");
            weight = serializedObject.FindProperty("weight");
            diameter = serializedObject.FindProperty("diameter");
            material = serializedObject.FindProperty("material");
            textureMode = serializedObject.FindProperty("textureMode");
            tiling = serializedObject.FindProperty("tiling");
            layer = serializedObject.FindProperty("layer");
            tag = serializedObject.FindProperty("tag");
            enableTreeMask = serializedObject.FindProperty("enableTreeMask");
            enableLargeObjectMask = serializedObject.FindProperty("enableLargeObjectMask");
            treeMaskWidth = serializedObject.FindProperty("treeMaskWidth");
            largeObjectMaskWidth = serializedObject.FindProperty("largeObjectMaskWidth");

            targetWires = WireBuilderUtilities.GetWiresUsingType((WireType)target);
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Rendering", EditorGUIUtility.IconContent("Mesh Icon").image));
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                EditorGUILayout.PropertyField(geometryType);
                EditorGUILayout.PropertyField(pointsPerMeter);
                if (geometryType.intValue == 1) EditorGUILayout.PropertyField(radialSegments);
            }

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Physical properties", EditorGUIUtility.IconContent("PhysicMaterial Icon").image));
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                EditorGUILayout.PropertyField(weight);
                EditorGUILayout.PropertyField(diameter);
            }

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Appearance", EditorGUIUtility.IconContent("Material Icon").image));
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(material);

                    using (new EditorGUI.DisabledGroupScope(material.objectReferenceValue == null))
                    {
                        if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.MaxWidth(60f)))
                        {
                            Selection.activeObject = material.objectReferenceValue;
                        }
                    }
                }
                if (material.objectReferenceValue == null) EditorGUILayout.HelpBox("Material must be assigned", MessageType.Error);
                if (geometryType.intValue == 0) EditorGUILayout.PropertyField(textureMode);
                if (geometryType.intValue == 1) EditorGUILayout.PropertyField(tiling);
            }


            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("GameObjects", EditorGUIUtility.IconContent("Prefab Icon").image));
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                layer.intValue = EditorGUILayout.LayerField("Layer", layer.intValue);
                tag.stringValue = EditorGUILayout.TagField("Tag", tag.stringValue);
            }

#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Vegetation Studio masking", EditorGUIUtility.IconContent("tree_icon_leaf").image));
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(enableTreeMask, new GUIContent("Remove trees"));
                if (enableTreeMask.boolValue) EditorGUILayout.PropertyField(treeMaskWidth);
                EditorGUILayout.PropertyField(enableLargeObjectMask, new GUIContent("Remove large objects"));
                if (enableLargeObjectMask.boolValue) EditorGUILayout.PropertyField(largeObjectMaskWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    if (targetWires == null) return;

                    foreach (Wire wire in targetWires)
                    {
                        wire.UpdateVegetationMask(wire.wireType);
                    }
                }
            }
#endif

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (targetWires == null) return;

                foreach (Wire wire in targetWires)
                {
                    wire.UpdateWire(false);
                }
            }

            WireBuilderGUI.DrawFooter();
        }
    }
}