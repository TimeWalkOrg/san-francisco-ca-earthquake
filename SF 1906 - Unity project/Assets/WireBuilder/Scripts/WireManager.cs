using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace WireBuilder
{
    [ExecuteInEditMode]
    // Maintains lists of components and holds several creation functions
    public static class WireManager
    {
        [Tooltip("Toggles additional visual gizmos when network objects are selected.\nSuch as the wire points, or connections between connectors.")]
        public static bool debug = false;

        [Tooltip("The connector groups current contained in the network")]
        public static List<WireConnectorGroup> Groups = new List<WireConnectorGroup>();
        [Tooltip("The connectors currently contained in the network")]
        public static List<WireConnector> Connectors = new List<WireConnector>();
        [Tooltip("The wires currently contained in the network")]
        public static List<Wire> Wires = new List<Wire>();

        #region Groups
        /// <summary>
        /// Creates a new WireConnectorGroup object. Object is instantiated as a prefab if the sourceObject is one. Used in Editor functions.
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static WireConnectorGroup CreateGroupObject(GameObject sourceObject)
        {
            if (sourceObject == null) Debug.Log("Failed to create connector group object, source object is null");

            if (sourceObject.GetComponent<WireConnectorGroup>() == null)
            {
                Debug.LogError("Source object has no WireConnectorGroup component", sourceObject);
                return null;
            }

#if UNITY_EDITOR
            //Get the prefab asset
            UnityEngine.Object prefabObject = PrefabUtility.GetCorrespondingObjectFromSource(sourceObject);
            bool isPrefab = prefabObject;

            if (prefabObject)
            {
                //Do not consider models a prefab
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.GetPrefabAssetType(sourceObject) == PrefabAssetType.Model) isPrefab = false;
#else
                if (PrefabUtility.GetPrefabType(sourceObject) == PrefabType.ModelPrefab ||
                    PrefabUtility.GetPrefabType(sourceObject) == PrefabType.ModelPrefabInstance) isPrefab = false;
#endif
            }

            //Debug.Log("Creating object from " + sourceObject.name + " " + (isPrefab ? "prefab" : "duplicate"));

            GameObject newGroup = isPrefab ? PrefabUtility.InstantiatePrefab(prefabObject) as GameObject : GameObject.Instantiate(sourceObject) as GameObject;

            if (!newGroup) Debug.LogError("FAILED TO INSTANTE");

            Undo.RegisterCreatedObjectUndo(newGroup, "Create connector group");
#else
        GameObject newGroup = (GameObject)GameObject.Instantiate(sourceObject);
#endif

            newGroup.name = sourceObject.name;
            newGroup.transform.parent = sourceObject.transform.parent;

            WireConnectorGroup group = newGroup.GetComponent<WireConnectorGroup>();

            return group;
        }

        /// <summary>
        /// Adds a WireConnectorGroup to the WireManager Groups list
        /// </summary>
        /// <param name="group"></param>
        public static void AddConnectorGroup(WireConnectorGroup group)
        {
            if (Groups.Contains(group) == false) Groups.Add(group);
        }

        /// <summary>
        /// Removes a WireConnectorGroup from the WireManager Groups list
        /// </summary>
        /// <param name="group"></param>
        public static void RemoveConnectorGroup(WireConnectorGroup group)
        {
            if (Groups.Contains(group) == true) Groups.Remove(group);
        }

        /// <summary>
        /// Updates the group and all wires connected to its connectors
        /// </summary>
        /// <param name="group"></param>
        public static void UpdateConnectorGroup(WireConnectorGroup group)
        {
            if (!group) return;

            //Update wire all connected wires
            foreach (WireConnector connector in group.connectors)
            {
                if (!connector) return;

                //Validate that each connector has a parent group
                connector.group = group;

                if (connector.wires != null) //In case of a new group
                {
                    foreach (Wire wire in connector.wires)
                    {
                        UpdateWire(wire, connector.wireType, false);
                    }
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(group);
#endif
        }

        /// <summary>
        /// Destroys the group and all connected wires
        /// </summary>
        /// <param name="group"></param>
        public static void DestroyConnectorGroup(WireConnectorGroup group)
        {
            if (!group) return;

            foreach (WireConnector connector in group.connectors)
            {
                //Delete connected wires
                for (int i = 0; i < connector.wires.Count; i++)
                {
                    if (connector.wires[i])
                    {
                        DestroyWire(connector.wires[i]);
                    }
                }
            }

            //Unregister group
            Groups.Remove(group);

            //Destroy group object
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(group.gameObject);
#else
            GameObject.DestroyImmediate(group.gameObject);
#endif
        }
        #endregion

        #region CONNECTORS
        public static WireConnector CreateConnectorObject()
        {
            GameObject connectorObj = new GameObject();
            connectorObj.name = "WireConnector";

            WireConnector connector = connectorObj.AddComponent<WireConnector>();

            return connector;
        }

        /// <summary>
        /// Adds a connector to the WireManager Connectors list
        /// </summary>
        /// <param name="connector"></param>
        public static void AddConnector(WireConnector connector)
        {
            //ValidateConnector(connector);

            if (!Connectors.Contains(connector)) Connectors.Add(connector);
        }

        /// <summary>
        /// Removes a connector from the WireManager Connectors list
        /// </summary>
        /// <param name="connector"></param>
        public static void RemoveConnector(WireConnector connector)
        {
            if (Connectors.Contains(connector)) Connectors.Remove(connector);
        }

        /// <summary>
        /// Updates all wires connected
        /// </summary>
        /// <param name="connector"></param>
        public static void UpdateConnector(WireConnector connector)
        {
            if (connector.wires != null) //In case of a new group
            {
                foreach (Wire wire in connector.wires)
                {
                    UpdateWire(wire, connector.wireType, false);
                }
            }
        }
        #endregion

        #region WIRES
        /// <summary>
        /// Creates a new wires based on the given connector's wire type. Automatically parents the object. Used in Editor functions.
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static Wire CreateWireObject(WireConnector connector)
        {
            if (!connector.wireType)
            {
                Debug.LogError("Trying to create wire from connect without a wire type");
                return null;
            }
            Wire newWire = WireGenerator.New(connector.wireType);

            //When connector is part of a group
            if (connector.group != null)
            {
                //If the group is parented to an object, parent the wire as well
                newWire.transform.parent = connector.group.transform.parent;
            }
            //parent wire to connector
            else
            {
                newWire.transform.parent = connector.transform;
            }

            return newWire;
        }

        /// <summary>
        /// Creates a new wire object of the type, without any connections. After assigning connections, the wire must be updated (enable updating wind as well)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Wire CreateWireObject(WireType type)
        {
            Wire newWire = WireGenerator.New(type);

            return newWire;
        }

        /// <summary>
        /// Creates a new wire between two connectors of the given type
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Wire CreateWireObject(WireConnector start, WireConnector end, WireType type)
        {
            Wire newWire = WireGenerator.New(type);

            newWire.startConnection = start;
            newWire.endConnection = end;

            newWire.UpdateWire(true);

            return newWire;
        }

        /// <summary>
        /// Adds a wire to the WireManager wires list. Always called in OnEnable of a wire component
        /// </summary>
        /// <param name="wire"></param>
        public static void AddWire(Wire wire)
        {
            if (!Wires.Contains(wire)) Wires.Add(wire);
        }

        /// <summary>
        /// Removes a wire from the WireManager wires list. Always called in OnDisable of a wire component
        /// </summary>
        /// <param name="wire"></param>
        public static void RemoveWire(Wire wire)
        {
            if (Wires.Contains(wire)) Wires.Remove(wire);
        }

        public static void UpdateWire(Wire wire, WireType type, bool updateWind = false)
        {
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.BeginSample("Update wire");
#endif
            //In case updating a connector's previously removed wire
            if (!wire) return;

            WireGenerator.Update(wire, type, updateWind);

#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        /// <summary>
        /// Destroys the wire GameObject and removes the wire from its start and end connectors
        /// </summary>
        /// <param name="wire"></param>
        public static void DestroyWire(Wire wire)
        {
            if (wire.startConnection != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(wire.startConnection, "Delete wire");
#endif
                wire.startConnection.wires.Remove(wire);
            }
            if (wire.endConnection != null)
            {
#if UNITY_EDITOR
                Undo.RecordObject(wire.endConnection, "Delete wire");
#endif
                wire.endConnection.wires.Remove(wire);
            }

#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(wire.gameObject);
#else
         GameObject.DestroyImmediate(wire.gameObject);
#endif
        }
        #endregion

        /// <summary>
        /// Updates every wire in the WireManager. Regenerates meshes/lines based on their WireType.
        /// </summary>
        /// <param name="updateWind">Regenerates the random wind data per wire</param>
        public static void UpdateAllWires(bool updateWind = false)
        {
            if (Connectors == null) return; //Brand new network

            foreach (WireConnector connector in Connectors)
            {
                if (!connector) return;

                foreach (Wire wire in connector.wires)
                {
                    wire.wireType = connector.wireType;

                    UpdateWire(wire, connector.wireType, updateWind);
                }
            }
        }

        #region Validations
        /// <summary>
        /// Regenerates new wind data for all wires in the WireManager
        /// </summary>
        public static void UpdateWireWind()
        {
            UpdateAllWires(true);
        }

        //Check for objects that have been removed manually and adjust the lists
        public static void ValidateAll()
        {
            if (Groups == null) return;

            //Remove any null references
            Groups.RemoveAll(WireConnectorgroup => WireConnectorgroup == null);
            Connectors.RemoveAll(WireConnector => WireConnector == null);
            Wires.RemoveAll(Wire => Wire == null);

            foreach (WireConnectorGroup group in Groups)
            {
                ValidateGroup(group);

                if (group.connectors == null) continue;

                foreach (WireConnector connector in group.connectors)
                {
                    ValidateConnector(connector);

                    if (connector.wires == null) continue;

                    foreach (Wire wire in connector.wires)
                    {
                        ValidateWire(wire);
                    }
                }
            }

            //Validate all wires individually, in case they aren't connected to any connectors and should be
            ValidateAllWires();
        }

        public static void ValidateAllGroups()
        {
            Groups.RemoveAll(WireConnectorgroup => WireConnectorgroup == null);

            foreach (WireConnectorGroup group in Groups)
            {
                foreach (WireConnector connector in group.connectors)
                {
                    if (connector == null) return;

                    foreach (Wire wire in connector.wires)
                    {
                        if (wire == null) connector.wires.Remove(wire);
                    }
                }
            }
        }

        private static void ValidateGroup(WireConnectorGroup group)
        {
            group.connectors.RemoveAll(WireConnector => WireConnector == null);

            foreach (WireConnector connector in group.connectors)
            {
                ValidateConnector(connector);
            }
        }

        private static void ValidateAllWires()
        {
            Wires.RemoveAll(Wire => Wire == null);

            foreach (Wire wire in Wires)
            {
                ValidateWire(wire);
            }
        }

        private static void ValidateWire(Wire wire)
        {
            if (Wires.Contains(wire) == false) Wires.Add(wire);

            //Check if wires connected to a connector are actually registered
            if (wire.startConnection)
            {
                if (wire.startConnection.wires.Contains(wire) == false)
                {
#if UNITY_EDITOR
                    EditorUtility.SetDirty(wire.startConnection);
#endif
#if WB_DEV
                    Debug.Log(wire.name + " was not registered to its start connector", wire.gameObject);
#endif
                    wire.startConnection.wires.Add(wire);
                }
            }
            if (wire.endConnection)
            {
                if (wire.endConnection.wires.Contains(wire) == false)
                {
#if UNITY_EDITOR
                    EditorUtility.SetDirty(wire.endConnection);
#endif
#if WB_DEV
                    Debug.Log(wire.name + " was not registered to its end connector", wire.gameObject);
#endif
                    wire.endConnection.wires.Add(wire);
                }
            }
        }

        private static void ValidateConnector(WireConnector connector)
        {
            if (Connectors.Contains(connector) == false) Connectors.Add(connector);

            if (connector.wires == null) return;

            connector.wires.RemoveAll(Wire => Wire == null);

            foreach (Wire wire in connector.wires)
            {
                ValidateWire(wire);
            }

        }
        #endregion
    }
}