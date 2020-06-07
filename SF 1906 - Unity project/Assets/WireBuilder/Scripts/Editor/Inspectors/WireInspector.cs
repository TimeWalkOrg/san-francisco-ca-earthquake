using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace WireBuilder
{
    [CustomEditor(typeof(Wire))]
    public class WireInspector : Editor
    {
        Wire wire;
        SerializedProperty sagOffset;

        private void OnEnable()
        {
            sagOffset = serializedObject.FindProperty("sagOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            wire = (Wire)target;

            EditorGUILayout.Space();

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Info", EditorGUIUtility.IconContent("console.infoicon").image));

            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Wire type: " + wire.wireType.name);

                    if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.MaxWidth(60f)))
                    {
                        Selection.activeObject = wire.wireType;
                    }

                }
                EditorGUILayout.LabelField("Length: " + Math.Round(wire.length, 2) + "m");
                //EditorGUILayout.LabelField("Sag depth: " + Math.Round(wire.sagDepth, 2) + "m");
                //EditorGUILayout.LabelField("Tension: " + wire.tension);

                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(sagOffset, new GUIContent("Sag amount"));
                    if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
                    {
                        sagOffset.floatValue = 0f;
                    }
                }

            }

            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Connections", WireBuilderGUI.ConnectorIcon.image));

            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                using (new EditorGUILayout.HorizontalScope())
                {

                    EditorGUILayout.LabelField("Start:", EditorStyles.boldLabel, GUILayout.MaxWidth(40f));
                    if (wire.startConnection)
                    {
                        EditorGUILayout.LabelField(wire.startConnection.gameObject.name + ((wire.startConnection.group) ? " (Group: " + wire.startConnection.group.name + ")" : ""));
                        if (GUILayout.Button("Select", GUILayout.MaxWidth(60f))) Selection.activeGameObject = wire.startConnection.gameObject;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(" None", EditorGUIUtility.IconContent("console.erroricon.sml").image));
                    }

                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("  End:", EditorStyles.boldLabel, GUILayout.MaxWidth(40f));
                    if (wire.endConnection)
                    {
                        EditorGUILayout.LabelField(wire.endConnection.gameObject.name + ((wire.endConnection.group) ? " (Group: " + wire.endConnection.group.name + ")" : ""));
                        if (GUILayout.Button("Select", GUILayout.MaxWidth(60f))) Selection.activeGameObject = wire.endConnection.gameObject;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(" None", EditorGUIUtility.IconContent("console.erroricon.sml").image));
                    }
                }

            }

            if (EditorGUI.EndChangeCheck())
            {
                wire.UpdateWire(false);
                serializedObject.ApplyModifiedProperties();
            }

            WireBuilderGUI.DrawFooter();
        }

        private void OnSceneGUI()
        {
            if (!wire) return;

            WireBuilderGUI.DrawSceneEditorWiresButton();

            if (WireManager.debug)
            {
                WireBuilderGUI.Scene.VisualizeWirePoints(wire);
            }
        }

    }
}