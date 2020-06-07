using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace WireBuilder
{
    [CustomEditor(typeof(WireConnector))]
    public class WireConnectorInspector : Editor
    {
        public static WireConnector lastActive;
        SerializedProperty wireType;
        WireConnector connector;

        private void OnEnable()
        {
            lastActive = (WireConnector)target;

            wireType = serializedObject.FindProperty("wireType");
        }
        public override void OnInspectorGUI()
        {
            connector = (WireConnector)target;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (connector.group)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button( new GUIContent(" Select group", WireBuilderGUI.GroupIcon.image), EditorStyles.miniButton, GUILayout.Height(25f), GUILayout.Width(100f)))
                {
                    Selection.activeGameObject = connector.group.gameObject;
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth -= 75f;
                EditorGUILayout.PropertyField(wireType);
                EditorGUIUtility.labelWidth += 75f;

                if (GUILayout.Button("Edit", GUILayout.MaxWidth(60f)))
                {
                    Selection.activeObject = wireType.objectReferenceValue;
                }
                if (GUILayout.Button("New", GUILayout.MaxWidth(60f)))
                {
                    WireBuilderEditor.CreateNewWireType();
                }
            }

            if (wireType.objectReferenceValue == null) EditorGUILayout.HelpBox("Connector must have a wire type assigned", MessageType.Error);

            if (connector.wires != null)
            {
                EditorGUILayout.Space();

                WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Connected wires", WireBuilderGUI.WireIcon.image));

                using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
                {

                    if (connector.wires.Count == 0) EditorGUILayout.LabelField("No wires connected", EditorStyles.miniLabel);
                    foreach (Wire wire in connector.wires)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (!wire)
                            {
                                EditorGUILayout.LabelField(new GUIContent(" Null wire", EditorGUIUtility.IconContent("console.erroricon.sml").image));
                            }
                            else
                            {
                                EditorGUILayout.PrefixLabel(wire.wireType.name + " (ID: " + wire.GetInstanceID() +")");

                                if (GUILayout.Button("Select", GUILayout.MaxWidth(50f)))
                                {
                                    Selection.activeGameObject = wire.gameObject;
                                }

                            }
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space(-10f);
            WireBuilderGUI.DrawFooter();
        }

        private void OnSceneGUI()
        {
            if (!connector) return;

            WireBuilderGUI.DrawSceneEditorWiresButton();

            if (connector.group)
            {
                WireManager.UpdateConnectorGroup(connector.group);
            }
            else
            {
                WireManager.UpdateConnector(connector);
            }

            if(connector.wireType == null) WireBuilderGUI.Scene.DrawObjectLabel(connector.transform.position, new GUIContent("No wire type!", WireBuilderGUI.ErrorIcon.image), true);

            if (Selection.activeGameObject != connector.gameObject) return;

            Handles.BeginGUI();
            Handles.CircleHandleCap(0, connector.transform.position, Quaternion.identity, 1f, EventType.Repaint);
            Handles.EndGUI();

            if (WireManager.debug == false) return;

            if (connector.wires != null)
            {
                foreach (Wire wire in connector.wires)
                {
                    if (!wire) return;

                    WireBuilderGUI.Scene.VisualizeWire(wire);
                }
            }
        }

    }
}