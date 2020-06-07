using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WireBuilder
{
    public class WireBuilderEditor : Editor
    {

        public static Wire previewWire;
        public static Wire[] previewWires;
        public static WireConnectorGroup previewCluster;
        public static WireConnectorGroup previewSourceObject;
        public static WireConnectorGroup previewServiceDrop;

        public static WireConnector mouseConnector;
        static Material previewMat;

        private static Material CreatePreviewMaterial()
        {
            WireBuilderUtilities.GetRenderPipeline();

            Shader shader = Shader.Find("Standard");
            if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.Builtin)
            {
                shader = Shader.Find("Standard");
                previewMat = new Material(shader);
                previewMat.color = new Color(0.19f, 0.77f, 1f, 0.25f);
            }
            if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.Lightweight)
            {
#if UNITY_2018_3_OR_NEWER  //LWRP 4.1.0 & 5.7.1
                shader = Shader.Find("Lightweight Render Pipeline/Unlit");
                previewMat = new Material(shader);
                previewMat.SetColor("_BaseColor", new Color(0.19f, 0.77f, 1f, 0.25f));
#endif

            }
            if (WireBuilderUtilities.CurrentPipeline == WireBuilderUtilities.RenderPipeline.Universal)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
                previewMat = new Material(shader);
                previewMat.SetColor("_BaseColor", new Color(0.19f, 0.77f, 1f, 0.25f));
            }

            return previewMat;
        }

        #region Groups
        private static void CreateClusterPreview(WireConnectorGroup sourceCluster)
        {
            if (previewCluster)
            {
                Debug.Log("Preview cluster already exists, removing");
                ClearClusterPreview();
            }

            if (!sourceCluster) Debug.LogError("Cannot create cluster preview, source cluster is null");

            //When not the first cluster
            if (sourceCluster)
            {
                //This is the object that actually gets duplicate once the preview ends. Duplicating the previewCluster means the preview material is retained
                previewSourceObject = sourceCluster;

                previewCluster = WireManager.CreateGroupObject(previewSourceObject.gameObject);

            }

            if (!previewCluster)
            {
                Debug.Log("Preview cluster failed to create");
                return;
            }

            Collider capCollider = previewCluster.gameObject.GetComponent<Collider>();
            if (capCollider) DestroyImmediate(capCollider);

            //previewCluster.gameObject.hideFlags = HideFlags.HideInInspector;
            previewCluster.name += " (Preview)";

            //When creating the first cluster, do not create wires
            if (sourceCluster)
            {
                //Exit on mismatching connector count
                if (previewCluster.connectors.Count != sourceCluster.connectors.Count) return;

                previewWires = new Wire[previewCluster.connectors.Count];

                for (int i = 0; i < previewWires.Length; i++)
                {
                    previewWires[i] = WireManager.CreateWireObject(sourceCluster.connectors[i]);
                    previewWires[i].name = "Wire (Preview)";
                    previewWires[i].startConnection = sourceCluster.connectors[i];
                    previewWires[i].endConnection = previewCluster.connectors[i];
                }
            }

            //A preview cluster
            MeshRenderer r = previewCluster.GetComponent<MeshRenderer>();

            if (r)
            {
                CreatePreviewMaterial();
                r.sharedMaterial = previewMat;
            }
        }

        public static void PreviewCluster(Vector3 position, WireConnectorGroup sourceCluster)
        {

            if (SceneView.lastActiveSceneView.in2DMode) position = new Vector3(position.x, position.y, sourceCluster.transform.position.z);

            if (!previewCluster)
            {
                //Debug.Log("Preview cluster created");
                CreateClusterPreview(sourceCluster);
            }

            if (sourceCluster)
            {
                if (previewCluster.connectors.Count != sourceCluster.connectors.Count)
                {
                    Debug.LogError("[WireManager] Source cluster does not have the same amount of connectors as the current prefab (" + sourceCluster.name + ")!");
                    return;
                }
            }

            //if (sourceCluster) Handles.DrawAAPolyLine(Texture2D.whiteTexture, 3, new Vector3[] { sourceCluster.transform.position, position });
            if (sourceCluster)
            {
                //Undo will reset the source cluster's rotation
                Undo.RecordObject(sourceCluster, "Source cluster");
                WireBuilderGUI.Scene.DrawDashedLine(sourceCluster.transform.position, position, 5f);
            }

            //Positioning
            previewCluster.transform.position = position;
            previewCluster.gameObject.hideFlags = HideFlags.HideInHierarchy;

            if (previewWires != null)
            {
                foreach (Wire wire in previewWires)
                {
                    WireManager.UpdateWire(wire, wire.wireType);
                }
            }

            //Rotate object towards source, flip rotation if direction is negative to avoid crossing wires
            if (sourceCluster)
            {
                float camFace = Vector3.Dot(sourceCluster.transform.forward, WireBuilderSceneGUI.sceneCam.transform.position);
                float face = Vector3.Dot(sourceCluster.transform.forward, previewCluster.transform.forward);

                if (face > 0 && camFace > 0)
                {
                    // previewCluster.transform.LookAt(sourceCluster.transform.position, Vector3.up);
                }
                else
                {
                }

                previewCluster.transform.LookAt(2 * previewCluster.transform.position - sourceCluster.transform.position, Vector3.up);
                sourceCluster.transform.LookAt(position, Vector3.up);

                //Lock X and Z rotation
                previewCluster.transform.localEulerAngles = new Vector3(0f, previewCluster.transform.localEulerAngles.y, 0f);
                sourceCluster.transform.localEulerAngles = new Vector3(0f, sourceCluster.transform.localEulerAngles.y, 0f);

                WireManager.UpdateConnectorGroup(sourceCluster);


            }

            if (sourceCluster && Vector3.Distance(sourceCluster.transform.position, position) > 35f && SceneView.lastActiveSceneView.in2DMode == false)
                UnityEditor.SceneView.lastActiveSceneView.LookAt(position, WireBuilderSceneGUI.sceneCam.transform.rotation, 75f);
        }

        public static void ClearClusterPreview()
        {
            if (previewCluster)
            {
                DestroyImmediate(previewCluster.gameObject);
            }

            if (previewWires != null)
            {
                foreach (Wire wire in previewWires)
                {
                    if (wire) DestroyImmediate(wire.gameObject);

                }

                previewWires = null;
            }

        }

        public static void ClearWirePreview()
        {
            if (previewWire)
            {
                previewWire.startConnection.wires.Remove(previewWire);
                if (previewWire.endConnection.wires != null) previewWire.endConnection.wires.Remove(previewWire);

                DestroyImmediate(mouseConnector);
                DestroyImmediate(previewWire.gameObject);
            }

            previewWire = null;
        }

        internal static void DeleteGroup(WireConnectorGroup group)
        {
            List<UnityEngine.Object> undoObjectList = new List<UnityEngine.Object>();

            undoObjectList.Add(group.gameObject);
            undoObjectList.Add(group);

            foreach (WireConnector connector in group.connectors)
            {
                if (!connector) continue;

                undoObjectList.Add(connector.gameObject);
                undoObjectList.Add(connector);

                Undo.RegisterCompleteObjectUndo(connector, "Delete group");

                foreach (Wire wire in connector.wires)
                {
                    if (!wire) continue;

                    undoObjectList.Add(wire.gameObject);
                    undoObjectList.Add(wire);

                    if (wire.startConnection) undoObjectList.Add(wire.startConnection);
                    if (wire.endConnection) undoObjectList.Add(wire.endConnection);
                }
            }

            Undo.RecordObjects(undoObjectList.ToArray(), "Delete group");

            WireManager.DestroyConnectorGroup(group);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            WireBuilderSceneGUI.Enabled = true;
        }

        private static void CleanGroup(WireConnectorGroup cluster)
        {
            List<WireConnector> childConnectors = new List<WireConnector>();

            foreach (Transform child in cluster.transform)
            {
                WireConnector connector = child.GetComponent<WireConnector>();
                if (connector != null)
                {
                    childConnectors.Add(connector);
                }
            }

            foreach (WireConnector connector in childConnectors)
            {
                DestroyImmediate(connector.gameObject);
            }

            DestroyImmediate(cluster);
        }
        #endregion

        #region Wires
        public static void PreviewWire(WireConnector sourceConnector, Vector3 mouseWorldPos)
        {
            if (!previewWire)
            {
                previewWire = WireManager.CreateWireObject(sourceConnector);
                previewWire.name = "Wire (Preview)";
                previewWire.gameObject.hideFlags = HideFlags.HideInHierarchy;

                WireManager.RemoveWire(previewWire);
            }
            if (mouseConnector == null)
            {
                mouseConnector = WireManager.CreateConnectorObject();
                mouseConnector.gameObject.hideFlags = HideFlags.HideInHierarchy;
                mouseConnector.name = "MouseConnector (Preview)";

                //Don't actually want this in the network. Will confuse the Wire Editor too.
                WireManager.RemoveConnector(mouseConnector);
            }

            previewWire.startConnection = sourceConnector;

            WireBuilderGUI.Scene.VisualizeWire(previewWire);

            //Bypass end connector world position so it's fixed to the mouse
            mouseConnector.transform.position = mouseWorldPos;
            previewWire.endConnection = mouseConnector;

            WireManager.UpdateWire(previewWire, sourceConnector.wireType);

            //UnityEditor.SceneView.lastActiveSceneView.LookAt(mouseWorldPos, WireSystemSceneGUI.sceneCam.transform.rotation, 100f);

        }

        //Convert a preview wire to a wire that's part of the network
        public static Wire CommitNewWire(WireConnector start, WireConnector end)
        {
            if (start == null || end == null)
            {
                Debug.LogWarning("Cannot commit wire with a null start or end connector");
                return null;
            }

            /* Now allowed, so groups can have pre-configured wires
            if (start.group == end.group)
            {
                Debug.LogWarning("Tried to attach a cable to the same cluster, this is not allowed");
                ClearWirePreview();
                return null;
            }
            */

            //Note: Wires will never be commited if the end connector has a mismatching wire type
            Wire newWire = WireManager.CreateWireObject(start);

            //During preview a temporary connector is attached to the mouse pos
            //Set the endConnector to the target connector
            newWire.startConnection = start;
            newWire.endConnection = end;

            start.wires.Add(newWire);
            EditorUtility.SetDirty(start);
            end.wires.Add(newWire);
            EditorUtility.SetDirty(end);

            newWire.UpdateWire(true);

            ClearWirePreview();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return newWire;
        }

        public static void CommitNewCluster()
        {
            if (previewCluster == null) return;

            //Note: Undo is handled in the CreateClusterObject and CommitNewWire functions

            //Create new object, since previewObject needs to be destroyed
            WireConnectorGroup newGroup = WireManager.CreateGroupObject(previewSourceObject.gameObject);

            newGroup.transform.position = previewCluster.transform.position;
            newGroup.transform.rotation = previewCluster.transform.rotation;

            newGroup.name.Replace("(Clone)", string.Empty);

            if (previewWires != null) //Disable for first cluster
            {
                //Move wire end connectors from preview to new end connectors
                for (int i = 0; i < previewWires.Length; i++)
                {
                    previewWires[i].endConnection = newGroup.connectors[i];
                    previewWires[i].endConnection.group = newGroup;

                    CommitNewWire(previewWires[i].startConnection, previewWires[i].endConnection);
                }
            }

            WireManager.AddConnectorGroup(newGroup);
            //Update wires of new group
            //Needed for the first group, otherwise its connector have a null parent group
            WireManager.UpdateConnectorGroup(newGroup);

            ClearClusterPreview();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void DeleteWire(Wire targetWire)
        {
            List<UnityEngine.Object> undoObjectList = new List<UnityEngine.Object>();

            if (!targetWire)
            {
                Debug.LogError(targetWire + " does not not exist");
                return;
            }
            //Connectors this wire is attached to
            if (targetWire.startConnection)
            {
                undoObjectList.Add(targetWire.startConnection);
            }
            if (targetWire.endConnection)
            {
                undoObjectList.Add(targetWire.endConnection);
            }

            undoObjectList.Add(targetWire);
            undoObjectList.Add(targetWire.gameObject);

            Undo.RecordObjects(undoObjectList.ToArray(), "Delete wire");

            WireManager.DestroyWire(targetWire);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        #endregion

        public static void ClearConnectorPreview()
        {
            if (mouseConnector) DestroyImmediate(mouseConnector.gameObject);
        }

        #region Utilities
        public static void CreateNewWireType()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateNewWireType>(), "New Wire Type.asset", null, null);
        }

        public static WireType CreateWireTypeAtPath(string path)
        {
            WireType wire = ScriptableObject.CreateInstance<WireType>();
            wire.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(wire, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return wire;
        }

        class DoCreateNewWireType : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var wire = CreateWireTypeAtPath(pathName);

                if (WireConnectorInspector.lastActive) WireConnectorInspector.lastActive.wireType = wire;

                ProjectWindowUtil.ShowCreatedAsset(wire);
            }
        }
        #endregion
    }
}