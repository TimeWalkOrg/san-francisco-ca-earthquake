//#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WireBuilder
{
    public class WireBuilderSceneGUI : Editor
    {
        public static Camera sceneCam;
        public SceneView sceneview;
        static WireBuilderSceneGUI _Instance;
        private static bool is2D;

        static WireBuilderSceneGUI Instance
        {
            get
            {
                if (_Instance == null) _Instance = ScriptableObject.CreateInstance<WireBuilderSceneGUI>();
                return _Instance;
            }
        }

        [InitializeOnLoadMethod]
        static void OnInitialize()
        {
            if (Enabled) Enable();
        }

        public static bool Enabled
        {
            get { return SessionState.GetBool("WIRE_EDITOR_GUI_ENABLED2", false); }
            set { SessionState.SetBool("WIRE_EDITOR_GUI_ENABLED2", value); }
        }

        public static void Enable()
        {
            Enabled = true;

            WireManager.ValidateAll();

            Undo.undoRedoPerformed += Instance.OnUndoRedo;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += Instance.OnScene;
#else
            SceneView.onSceneGUIDelegate += Instance.OnScene;
#endif

        }

        public static void Disable()
        {
            Enabled = false;

            Undo.undoRedoPerformed -= Instance.OnUndoRedo;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= Instance.OnScene;
#else
            SceneView.onSceneGUIDelegate -= Instance.OnScene;
#endif
        }

        WireConnector sourceConnector;
        WireConnector targetConnector;
        WireConnectorGroup currentGroup;
        Wire currentWire;
        Wire selectedWire;

        //Mouse
        Vector3 mouseScreenPos;
        RaycastHit mouseHit;
        Ray worldRay;

        //Actions
        private bool holdingCtrl;
        private bool holdingShift;
        private bool draggingLeftMouse;
        private bool mouseOverGroup;
        private bool mouseOverConnector;
        private bool mouseOverWire;
        private bool connectableConnector;
        private bool draggingFromObject;
        private bool hasStartWireSagPos;
        private Vector3 wireSagStartPos;
        //Meta
        private bool toolRestored;
        private Event curEvent;
        private float screenHeight;
        private Wire lastSelectedWire;

        private bool ShowHelpers
        {
            get { return EditorPrefs.GetBool("WIRE_EDITOR_SHOW_HELPERS", true); }
            set { EditorPrefs.SetBool("WIRE_EDITOR_SHOW_HELPERS", value); }
        }
        private bool EditGroup
        {
            get { return SessionState.GetBool("WIRE_EDITOR_EDIT_GROUPS", true); }
            set { SessionState.SetBool("WIRE_EDITOR_EDIT_GROUPS", value); }
        }

        private bool EditConnector
        {
            get { return SessionState.GetBool("WIRE_EDITOR_EDIT_CONNECTORS", true); }
            set { SessionState.SetBool("WIRE_EDITOR_EDIT_CONNECTORS", value); }
        }
        private bool EditWires
        {
            get { return SessionState.GetBool("WIRE_EDITOR_EDIT_WIRES", true); }
            set { SessionState.SetBool("WIRE_EDITOR_EDIT_WIRES", value); }
        }

        void OnScene(SceneView currentSceneview)
        {
            if (!Enabled) return;

            this.sceneview = currentSceneview;
            sceneCam = sceneview.camera;
            curEvent = Event.current;

            is2D = sceneview.in2DMode;

#if !DEBUG
            //Debug GUI
            Handles.BeginGUI();
            Rect debugRect = new Rect(10, 10, 300, 175f);
            GUILayout.BeginArea(debugRect, EditorStyles.helpBox);
            {
                GUILayout.Label("Mouse over cluster: " + mouseOverGroup);
                GUILayout.Label("Mouse over connector: " + mouseOverConnector);
                GUILayout.Label("Source connector: " + sourceConnector);
                GUILayout.Label("target connector: " + targetConnector);
                GUILayout.Label("Wire: " + currentWire);

            }
            GUILayout.EndArea();
            Handles.EndGUI();
#endif


            Handles.BeginGUI();
            Rect pixelRect = EditorGUIUtility.PixelsToPoints(Camera.current.pixelRect);

            screenHeight = pixelRect.height;
            Rect windowRect = new Rect(pixelRect.width - 250 - 10, screenHeight - 85, 250, 75f);

            GUILayout.BeginArea(windowRect);
            {
                GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
                style.richText = true;

                using (new GUILayout.HorizontalScope())
                {
                    ShowHelpers = GUILayout.Toggle(ShowHelpers, new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml").image, "Toggle context sensitive instructions"), EditorStyles.toolbarButton, GUILayout.MaxWidth(30f));

                    GUILayout.Label(new GUIContent("Wire Editor"), EditorStyles.toolbarButton, GUILayout.MinWidth(140));
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_winbtn_win_close").image, "Close editor"), EditorStyles.toolbarButton, GUILayout.MaxWidth(30f)))
                    {
                        Disable();
                    }
                }
                GUILayout.Space(-3f);

                using (new GUILayout.HorizontalScope())
                {
                    EditGroup = GUILayout.Toggle(EditGroup, new GUIContent(" Groups", WireBuilderGUI.GroupIcon.image, "Show groups and allow editing"), EditorStyles.miniButtonMid, GUILayout.MaxHeight(20f));
                    EditConnector = GUILayout.Toggle(EditConnector, new GUIContent(" Connectors", WireBuilderGUI.ConnectorIcon.image, "Show connectors and allow editing"), EditorStyles.miniButtonMid, GUILayout.MaxHeight(20f));
                    EditWires = GUILayout.Toggle(EditWires, new GUIContent(" Wires", WireBuilderGUI.WireIcon.image, "Show wires and allow editing"), EditorStyles.miniButtonMid, GUILayout.MaxHeight(20f));
                }
                GUILayout.Space(-2f);
                GUILayout.BeginVertical(EditorStyles.textArea);
                GUILayout.Label("Hold <b>CTRL</b> to edit", style);
                GUILayout.Label("Hold <b>CTRL+SHIFT</b> to delete", style);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
            Handles.EndGUI();

            #region Input
            //When holding down left shift
            holdingShift = (curEvent.shift);
            //When pressing down control
            holdingCtrl = curEvent.control;

            //When holding down left mouse button
            if (curEvent.isMouse && curEvent.type == EventType.MouseDrag && curEvent.button == 0)
            {
                draggingLeftMouse = true;
            }
            if (curEvent.isMouse && curEvent.type == EventType.MouseUp && curEvent.button == 0)
            {
                draggingLeftMouse = false;
            }
            #endregion

            if (!holdingCtrl)
            {
                if (!toolRestored)
                {
                    Tools.current = Tool.Move;
                    toolRestored = true;
                    Reset();
                }
                return;
            }

            //Tool preresiquites
            Tools.current = Tool.None;

            //Mute built in transform tools
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            toolRestored = false;

            //Mouse
            mouseScreenPos = Event.current.mousePosition;
            worldRay = HandleUtility.GUIPointToWorldRay(mouseScreenPos);

            if (HandleUtility.RaySnap(worldRay) != null) mouseHit = (RaycastHit)HandleUtility.RaySnap(worldRay);

            if (is2D)
            {
                mouseHit.point = new Vector3(mouseScreenPos.x, screenHeight - mouseScreenPos.y, 0);
                mouseHit.point = sceneCam.ScreenToWorldPoint(mouseHit.point);
            }

            WireBuilderGUI.Scene.SetColor((holdingShift && !draggingLeftMouse) ? WireBuilderGUI.Scene.DeleteColor : WireBuilderGUI.Scene.EditColor);

            //Cursor.SetCursor((Texture2D)EditorGUIUtility.IconContent("d_ViewToolMove").image, mouseScreenPos, CursorMode.Auto);
            //Cursor.visible = true;
            //if (mouseOverCluster || mouseOverConnector || mouseOverWire) Cursor.visible = false;

            DoGroups();
            DoConnectors();
            DoWires();

            #region Selection actions
            //When duplicating a cluster
            if (currentGroup)
            {
                if (holdingCtrl)
                {
                    //When dragging the mouse way from the last selected cluster
                    if (draggingLeftMouse)
                    {
                        WireBuilderEditor.PreviewCluster(mouseHit.point, currentGroup);

                        if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(mouseHit.point, new GUIContent("Release to commit", WireBuilderGUI.ConnectIcon.image));

                    }
                    //When mouse stops moving
                    else
                    {
                        //Mouse button releases
                        if (curEvent.isMouse && curEvent.type == EventType.MouseUp && curEvent.button == 0)
                        {
                            WireBuilderEditor.CommitNewCluster();

                            Reset();
                        }
                    }
                }

            }

            //When draggin a wire
            if (sourceConnector)
            {
                //When dragging the mouse way from the last selected cluster
                WireBuilderEditor.PreviewWire(sourceConnector, mouseHit.point);

                if (!connectableConnector && ShowHelpers) WireBuilderGUI.Scene.DrawMouseWorldLabel(mouseScreenPos, new GUIContent("Drag onto connector", WireBuilderGUI.DragIcon.image));
            }

            if (selectedWire)
            {
                if (!holdingShift)
                {
                    lastSelectedWire = selectedWire;

                    //Adjusting wire sagging
                    if (draggingLeftMouse)
                    {
                        if (!hasStartWireSagPos)
                        {
                            wireSagStartPos = selectedWire.transform.position;
                            wireSagStartPos.y -= selectedWire.sagOffset;
                            hasStartWireSagPos = true;

                            Undo.RecordObject(selectedWire, "Adjusted wire sag");
                        }

                        Vector3 sagScreenPos = sceneCam.WorldToScreenPoint(wireSagStartPos);

                        //Screen space distance between starting point and current mouse pos
                        float pixelDist = (mouseScreenPos.y - sagScreenPos.y) / (Screen.height * 0.2f);

                        //World space translation
                        Vector3 newSagPos = wireSagStartPos + (Vector3.up * pixelDist);


                        WireBuilderGUI.Scene.DrawDottedLine(wireSagStartPos, newSagPos, 0.5f);
                        Handles.DrawSolidDisc(wireSagStartPos, Vector3.up, 0.1f);
                        Handles.DrawSolidDisc(newSagPos, Vector3.up, 0.2f);


                        selectedWire.sagOffset = newSagPos.y - selectedWire.transform.position.y;

                        selectedWire.UpdateWire(false);
                    }
                    else
                    {
                        //WireManagerGUI.Scene.DrawObjectLabel(currentWire.transform.position, new GUIContent("Drag down", WireManagerGUI.DragIcon.image));

                    }

                    hasStartWireSagPos = false;

                }
            }
            #endregion

        }

        private void OnUndoRedo()
        {
            //Debug.Log("Undo/Redo performed");

            if (lastSelectedWire)
            {
                lastSelectedWire.UpdateWire(false);
            }
        }

        private void DoGroups()
        {
            if (WireManager.Groups != null)
            {
                List<WireConnectorGroup> groups = WireManager.Groups.Where(WireConnectorGroup => WireConnectorGroup != null).ToList();
                foreach (WireConnectorGroup group in groups)
                {
                    if (!group || !EditGroup || !group.gameObject.activeInHierarchy) continue;

                    float handleSize = group.handleSize;

                    handleSize = ScaleByDistance(group.transform.position, 10f, handleSize, handleSize * 0.8f);

                    mouseOverGroup = false;
                    //Do not update current object when dragging the mouse (out of the selection rect)
                    if (!draggingLeftMouse)
                    {
                        mouseOverGroup = HandleUtility.DistanceToCircle(group.transform.position, handleSize) < handleSize;
                        WireBuilderGUI.Scene.SetOpacity(mouseOverGroup ? 0.66f : 0.25f);
                        WireBuilderGUI.Scene.DrawSquare(group.transform.position, handleSize, mouseOverGroup);
                    }


                    if (mouseOverGroup && !draggingLeftMouse)
                    {
                        currentGroup = group;
                        currentWire = null;

                        //Handles.BeginGUI();
                        if (holdingShift)
                        {
                            //Delete icon
                            if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(currentGroup.transform.position, new GUIContent("Delete group", WireBuilderGUI.DeleteIcon.image));
                        }
                        else
                        {
                            //Show drag signal
                            if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(currentGroup.transform.position, new GUIContent("Duplicate group", WireBuilderGUI.DuplicateIcon.image));
                        }
                        //Handles.EndGUI();
                    }

                    //When mouse over a cluster
                    if (currentGroup)
                    {
                        if (holdingShift)
                        {
                            //Mouse button releases
                            if (curEvent.isMouse && curEvent.type == EventType.MouseUp && curEvent.button == 0)
                            {
                                WireBuilderEditor.DeleteGroup(currentGroup);

                                Reset();
                            }
                        }
                    }
                }
            }

        }

        private void DoConnectors()
        {
            if (WireManager.Connectors == null || !EditConnector) return;

            List<WireConnector> connectors = WireManager.Connectors.Where(WireConnector => WireConnector != null).ToList();
            foreach (WireConnector connector in connectors)
            {
                //Note: Connectors cannot be deleted
                if (!connector || !EditConnector || holdingShift || !connector.gameObject.activeInHierarchy) continue;

                //Disable connector and wire controls when creating a new cluster
                if (currentGroup && draggingLeftMouse) continue;

                float handleSize = ScaleByDistance(connector.transform.position, 5f, 0.5f, 0.5f * 0.8f);
                mouseOverConnector = HandleUtility.DistanceToCircle(connector.transform.position, handleSize) < handleSize;
                //handleSize = mouseOverConnector ? Mathf.Lerp(handleSize, handleSize * 0.9f, WireManagerGUI.Scene.Sin()) : handleSize;

                //Idle styling
                WireBuilderGUI.Scene.SetOpacity(mouseOverConnector ? 0.66f : 0.25f);
                //Do not display circle for source connector when dragging out wire
                if (connector != sourceConnector) WireBuilderGUI.Scene.DrawCircle(connector.transform.position, handleSize, mouseOverWire);


                targetConnector = null;

                if (mouseOverConnector)
                {
                    currentGroup = null;
                    currentWire = null;
                    selectedWire = null;

                    //Hovering over a connector in edit mode
                    if (!sourceConnector)
                    {
                        if (connector.wireType == null)
                        {
                            WireBuilderGUI.Scene.DrawObjectLabel(connector.transform.position, new GUIContent("Missing wire type!", WireBuilderGUI.ErrorIcon.image), true);
                        }
                        else
                        {
                            if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(connector.transform.position, new GUIContent("Drag out new " + connector.wireType.name, WireBuilderGUI.DragIcon.image));

                            //When holding down mouse over a connector, makes a selection
                            if (curEvent.isMouse && curEvent.type == EventType.MouseDown && curEvent.button == 0)
                            {
                                sourceConnector = connector;
                            }
                        }
                    }

                    //Mouse over other connector when dragging out
                    if (sourceConnector && sourceConnector != connector)
                    {
                        targetConnector = connector;
                        connectableConnector = true;
                        if (connectableConnector && ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(connector.transform.position, new GUIContent("Release to connect", WireBuilderGUI.ConnectIcon.image));

                        //Releasing left mouse button
                        if (curEvent.isMouse && curEvent.type == EventType.MouseUp && curEvent.button == 0)
                        {
                            WireBuilderEditor.CommitNewWire(sourceConnector, targetConnector);

                            Reset();
                        }
                    }
                    else
                    {
                        connectableConnector = false;
                    }
                }
            }
        }

        private void DoWires()
        {
            if (WireManager.Wires != null)
            {
                List<Wire> wires = WireManager.Wires.Where(Wire => Wire != null).ToList();
                foreach (Wire wire in wires)
                {
                    if (wire.gameObject.activeInHierarchy == false) continue;

                    if (wire.startConnection == null)
                    {
                        WireBuilderGUI.Scene.DrawObjectLabel(wire.startPos, new GUIContent(" Missing start connector", EditorGUIUtility.IconContent("console.erroricon").image));
                    }
                    if (wire.endConnection == null)
                    {
                        WireBuilderGUI.Scene.DrawObjectLabel(wire.endPos, new GUIContent(" Missing end connector", EditorGUIUtility.IconContent("console.erroricon").image));
                    }

                    if (!EditWires) continue;

                    float handleSize = ScaleByDistance(wire.transform.position, 10f, 0.5f, 0.5f * 0.8f);
                    mouseOverWire = HandleUtility.DistanceToCircle(wire.transform.position, handleSize) < handleSize;
                    WireBuilderGUI.Scene.SetOpacity(mouseOverWire || selectedWire ? 0.66f : 0.25f);
                    if (!draggingLeftMouse) WireBuilderGUI.Scene.DrawCircle(wire.transform.position, handleSize, mouseOverWire);

                    //Style idle control
                    if (!draggingLeftMouse && !holdingShift)
                    {
                        if (wire.sagOffset > 0)
                        {
                            WireBuilderGUI.Scene.DrawDottedLine(wire.transform.position, wire.transform.position + (Vector3.up * wire.sagOffset), 0.5f);
                        }
                    }

                    currentWire = null;
                    if (mouseOverWire && !draggingLeftMouse)
                    {
                        currentGroup = null;
                        sourceConnector = null;
                        targetConnector = null;
                        currentWire = wire;

                        if (holdingShift)
                        {
                            if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(currentWire.transform.position, new GUIContent("Delete", WireBuilderGUI.DeleteIcon.image));

                            //Mouse button pressed
                            if (curEvent.isMouse && curEvent.type == EventType.MouseDown && curEvent.button == 0)
                            {
                                WireBuilderEditor.DeleteWire(currentWire);

                                Reset();
                            }
                        }
                        else
                        {
                            if (ShowHelpers) WireBuilderGUI.Scene.DrawObjectLabel(currentWire.transform.position, new GUIContent("Drag down", WireBuilderGUI.DragIcon.image));

                            //Mouse button pressed
                            if (curEvent.isMouse && curEvent.type == EventType.MouseDown && curEvent.button == 0)
                            {
                                selectedWire = currentWire;
                            }
                        }
                    }

                }
            }
        }

        private float ScaleByDistance(Vector3 worldPos, float distNear, float sizeNear, float sizeFar)
        {
            float dist = Vector3.Distance(worldPos, sceneCam.transform.position);

            //Do not downscale within this range
            dist += distNear;

            //Scale by 80%
            float scale = Mathf.Lerp(sizeNear, sizeFar, dist);

            return scale;
        }

        public static float WorldCircleRadius(Vector3 position, float diameter)
        {

            Vector2 screenCenter = HandleUtility.WorldToGUIPoint(position);

            var screenEdge = HandleUtility.WorldToGUIPoint(position + sceneCam.transform.right * diameter);
            float radius = (screenCenter - screenEdge).magnitude;

            return radius;
        }

        private void Reset()
        {
            //Safety cleanup
            WireBuilderEditor.ClearClusterPreview();
            WireBuilderEditor.ClearWirePreview();
            WireBuilderEditor.ClearConnectorPreview();

            currentGroup = null;
            currentWire = null;
            selectedWire = null;
            sourceConnector = null;
            targetConnector = null;
            draggingLeftMouse = false;
        }
    }
}