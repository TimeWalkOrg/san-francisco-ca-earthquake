using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace WireBuilder
{
    [CustomEditor(typeof(WireConnectorGroup))]
    public class WireConnectorGroupInspector : Editor
    {
        WireConnectorGroup group;

        private bool isPrefab;

        private void OnEnable()
        {
            group = (WireConnectorGroup)target;

#if UNITY_2018_3_OR_NEWER
            isPrefab = PrefabUtility.GetPrefabInstanceHandle(group.gameObject) != null;
#else
            isPrefab = PrefabUtility.GetPrefabObject(group.gameObject) != null;
#endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            //Disable fields when inspecting prefab from the project browser
            if (group.gameObject.scene.IsValid())
            {

            }

            EditorGUILayout.Space();

            List<WireConnector> connectors = group.connectors.Where(WireConnector => WireConnector != null).ToList();

            int index = 0;
            WireBuilderGUI.ParameterGroup.DrawHeader(new GUIContent("Connectors", WireBuilderGUI.ConnectorIcon.image));

            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                foreach (WireConnector connector in connectors)
                {
                    DrawConnector(index, connector);
                    index++;
                }
                if (GUILayout.Button(new GUIContent(" Add", EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Insert new connector before"), EditorStyles.miniButtonMid, GUILayout.Width(60f)))
                {
                    AddConnector();
                }
            }

            if (isPrefab)
            {
                //EditorGUILayout.HelpBox("Adding or removing connectors will break any networks that use this prefab!", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space(-10f);

            WireBuilderGUI.DrawFooter();

        }

        private void DrawConnector(int i, WireConnector connector)
        {
            SerializedObject connectorObject = new SerializedObject(connector);
            SerializedProperty wireType = connectorObject.FindProperty("wireType");

            connectorObject.Update();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.MaxWidth(50f)))
                {
                    SelectConnector(connector);
                }
                if (GUILayout.Button(connector.gameObject.name, EditorStyles.toolbarButton))
                {

                }

                EditorGUI.BeginDisabledGroup(i == 0);
                {
                    if (GUILayout.Button(new GUIContent("▲", null, "Move up"), EditorStyles.toolbarButton, GUILayout.Width(30f)))
                    {
                        group.connectors.RemoveAt(i);
                        group.connectors.Insert(i - 1, connector);
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(i == group.connectors.Count - 1);
                {
                    if (GUILayout.Button(new GUIContent("▼", null, "Move down"), EditorStyles.toolbarButton, GUILayout.Width(30f)))
                    {
                        group.connectors.RemoveAt(i);
                        group.connectors.Insert(i + 1, connector);
                    }
                }
                EditorGUI.EndDisabledGroup();

                //Remove
                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Remove connector"), EditorStyles.toolbarButton, GUILayout.Width(30f)))
                {
                    if (EditorUtility.DisplayDialog("Delete connector", "This operation cannot be undone, continue?", "OK", "Cancel"))
                    {
                        DestroyImmediate(group.connectors[i].gameObject);
                        group.connectors.RemoveAt(i);
                    }
                }
            }

            if (!connector)
            {
                EditorGUILayout.HelpBox("Null connector", MessageType.Error);
                return;
            }

            GUILayout.Space(-2.5f);
            using (new EditorGUILayout.VerticalScope(WireBuilderGUI.ParameterGroup.Section))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth -= 75f;
                    EditorGUI.BeginChangeCheck();
                    wireType.objectReferenceValue = (WireType)EditorGUILayout.ObjectField("Wire Type", wireType.objectReferenceValue, typeof(WireType), true);
                    EditorGUIUtility.labelWidth += 75f;

                    using (new EditorGUI.DisabledGroupScope(wireType.objectReferenceValue == null))
                    {
                        if (GUILayout.Button("Edit", GUILayout.MaxWidth(60f)))
                        {
                            Selection.activeObject = wireType.objectReferenceValue;
                        }
                    }
                    if (GUILayout.Button("New", GUILayout.MaxWidth(60f)))
                    {
                        WireConnectorInspector.lastActive = connector;
                        WireBuilderEditor.CreateNewWireType();
                    }
                }
                if (connector.wireType == null) EditorGUILayout.HelpBox("Connector must have a Wire Type assigned", MessageType.Error);

                connector.transform.localPosition = EditorGUILayout.Vector3Field("Position", connector.transform.localPosition);
            }


            connectorObject.ApplyModifiedProperties();

        }

        private void AddConnector()
        {
            GameObject connectorObj = new GameObject();
            connectorObj.transform.parent = group.transform;
            connectorObj.transform.rotation = Quaternion.identity;
            connectorObj.transform.localPosition = Vector3.zero;
            connectorObj.name = "Connector" + group.connectors.Count;

            WireConnector newConnector = connectorObj.AddComponent<WireConnector>();
            newConnector.group = group;

            group.connectors.Add(newConnector);

            SelectConnector(newConnector);
        }

        private void SelectConnector(WireConnector connector)
        {
            Selection.activeGameObject = connector.gameObject;

            //Set focus to scene view
#if !UNITY_2018_2_OR_NEWER
                EditorApplication.ExecuteMenuItem("Window/Scene");
#else
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
#endif
            if (SceneView.lastActiveSceneView)
            {
                UnityEditor.SceneView.lastActiveSceneView.LookAt(connector.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation, 25f);
            }
            return;
        }
        WireConnector selectedConnector;
        private Event curEvent;

        private void OnSceneGUI()
        {
            if (!group) return;

            WireBuilderGUI.DrawSceneEditorWiresButton();

            WireManager.UpdateConnectorGroup(group);

            foreach (WireConnector connector in group.connectors)
            {
                if (!connector) return;

                if (connector.wires == null) return;

                if (WireManager.debug)
                {
                    foreach (Wire wire in connector.wires)
                    {
                        WireBuilderGUI.Scene.VisualizeWire(wire);
                    }
                }
            }
        }
    }
}