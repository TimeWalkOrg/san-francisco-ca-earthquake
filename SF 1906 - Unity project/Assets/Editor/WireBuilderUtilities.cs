using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WireBuilder
{
    public class WireBuilderUtilities : Editor
    {
        public enum RenderPipeline
        {
            Builtin,
            Lightweight,
            Universal,
            HighDefinition
        }

        public const string BuiltInShader = "FX/Wire Animated (Standard)";
        public const string URPShader = "Shader Graphs/Wire_Animated_URP";

        public static RenderPipeline CurrentPipeline;

        public static void GetRenderPipeline()
        {
#if UNITY_2018_1_OR_NEWER

#if UNITY_2019_1_OR_NEWER //Render pipeline is no longer expiremental
            UnityEngine.Rendering.RenderPipelineAsset renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
#else
            UnityEngine.Experimental.Rendering.RenderPipelineAsset renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
#endif

            if (renderPipelineAsset)
            {
                if (renderPipelineAsset.name.Contains("Lightweight") || renderPipelineAsset.name.Contains("LWRP")) { CurrentPipeline = RenderPipeline.Lightweight; }
                if (renderPipelineAsset.name.Contains("Universal") || renderPipelineAsset.name.Contains("URP")) { CurrentPipeline = RenderPipeline.Universal; }
                if (renderPipelineAsset.name.Contains("Definition") || renderPipelineAsset.name.Contains("HD")) { CurrentPipeline = RenderPipeline.HighDefinition; }
            }
            else { CurrentPipeline = RenderPipeline.Builtin; }

#if WB_DEV
            Debug.Log("<b>" + AssetInfo.ASSET_NAME + "</b> Pipeline active: " + CurrentPipeline.ToString());
#endif
#else
            CurrentPipeline = RenderPipeline.Standard;
#endif
        }

        public static void SetupMaterials()
        {
            GetRenderPipeline();

            Shader wireShader = Shader.Find(BuiltInShader);
            Shader objectShader = Shader.Find("Standard");

            if (CurrentPipeline == RenderPipeline.Builtin)
            {
                wireShader = Shader.Find(BuiltInShader);
                if (!wireShader) Debug.LogError("The wire shader for the standard Render Pipeline could not be found. Be sure to import all the package contents from the asset store");

                objectShader = Shader.Find("Standard");
            }
            if (CurrentPipeline == RenderPipeline.Lightweight)
            {
                wireShader = Shader.Find(URPShader);
                if (!wireShader) Debug.LogError("The wire shader for the <b>Lightweight Render Pipeline</b> could not be found. Be sure to import all the package contents from the asset store");

#if !UNITY_2018_3_OR_NEWER //LWRP 3.0.0
                objectShader = Shader.Find("LightweightPipeline/Standard (Physically Based)");
#endif
#if UNITY_2018_3_OR_NEWER  //LWRP 4.1.0 & 5.7.1
                objectShader = Shader.Find("Lightweight Render Pipeline/Lit");
#endif
            }
            if (CurrentPipeline == RenderPipeline.Universal)
            {
                wireShader = Shader.Find(URPShader);
                if (!wireShader) Debug.LogError("The wire shader for the <b>Universal Render Pipeline</a> could not be found. Be sure to import all the package contents from the asset store");

                objectShader = Shader.Find("Universal Render Pipeline/Lit");
            }

            //get wire type materials
            WireType[] wireTypes = GetWireTypesInProject();

            if (wireTypes != null)
            {
                foreach (WireType type in wireTypes)
                {
                    type.material.shader = wireShader;

                    EditorUtility.SetDirty(type.material);
                }

                Debug.Log(wireTypes.Length + " wire materials were configured");
            }

            //get demo materials

            AssetInfo.GetRootFolder();

            string[] GUIDs = AssetDatabase.FindAssets("t: material", new string[] { AssetInfo.PACKAGE_ROOT_FOLDER + "_Demo/Materials" });

            if (GUIDs.Length > 0)
            {
                Material[] mats = new Material[GUIDs.Length];

                for (int i = 0; i < mats.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(GUIDs[i]);

                    mats[i] = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));

                    mats[i].shader = objectShader;

                    EditorUtility.SetDirty(mats[i]);

                }
                Debug.Log(GUIDs.Length + " demo materials were configured");

            }
        }

        public static WireType[] GetWireTypesInProject()
        {
            string[] GUIDs = AssetDatabase.FindAssets("t: WireType");

            if (GUIDs.Length == 0) return null;

            WireType[] wireTypes = new WireType[GUIDs.Length];

            for (int i = 0; i < wireTypes.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUIDs[i]);

                wireTypes[i] = (WireType)AssetDatabase.LoadAssetAtPath(path, typeof(WireType));
            }

            return wireTypes;
        }

        public static List<Wire> GetWiresUsingType(WireType wireType)
        {
            Wire[] allWires = FindObjectsOfType<Wire>();
            List<Wire> wires = new List<Wire>();

            for (int i = 0; i < allWires.Length; i++)
            {
                if (allWires[i].wireType == wireType) wires.Add(allWires[i]);
            }

            return wires;
        }

       
        /*
        public class ReplaceClusters : EditorWindow
        {
            //[MenuItem("Tools/Wire Network/Replace clusters")]
            public static void OpenWindow()
            {
                ReplaceClusters window = (ReplaceClusters)GetWindow(typeof(ReplaceClusters), true, "Replace clusters");

                window.Show();
            }

            private static GameObject[] clusters = new GameObject[0];
            private static Object targetPrefab;
            void OnGUI()
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Replaces the selected WireConnectorCluster objects with another WireConnectorCluster prefab\n\nWire connections will be rerouted", MessageType.Info);
                EditorGUILayout.Space();

                if (GUILayout.Button("Choose selection"))
                {
                    clusters = Selection.gameObjects;
                }

                if (clusters.Length > 0) EditorGUILayout.LabelField("Selected " + clusters.Length + " objects");
                targetPrefab = EditorGUILayout.ObjectField("With (prefab)", targetPrefab, typeof(Object), false);

                using (new EditorGUI.DisabledGroupScope(targetPrefab == null))
                {
                    if (GUILayout.Button("Replace"))
                    {
                        ReplaceSelectedClustersWithPrefab();
                    }
                }
            }

            private static void ReplaceSelectedClustersWithPrefab()
            {
                foreach (GameObject clusterObj in clusters)
                {
                    WireConnectorGroup cluster = clusterObj.GetComponent<WireConnectorGroup>();

                    if (!cluster) continue;

                    GameObject newClusterObj = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);
                    newClusterObj.name = newClusterObj.name.Replace("(Clone)", string.Empty);

                    WireConnectorGroup newCluster = newClusterObj.GetComponent<WireConnectorGroup>();

                    newCluster.connectors = new List<WireConnector>(cluster.connectors.Count);

                    foreach (WireConnector connector in cluster.connectors)
                    {
                        foreach (Wire wire in connector.wires)
                        {
                            //wire.startConnector = 
                        }
                    }
                }
            }
        }
        */
        private static List<Wire> reconnectedWires = new List<Wire>();

        internal static void ReconnectWires()
        {
            reconnectedWires.Clear();

            Wire[] wires = GameObject.FindObjectsOfType<Wire>();

            Debug.Log("Checking " + wires.Length + " wires...");

            foreach (Wire wire in wires)
            {
                if (wire.startConnection) ReconnectWireToConnector(wire, wire.startConnection);
                if (wire.endConnection) ReconnectWireToConnector(wire, wire.endConnection);

                if (!wire.startConnection || !wire.endConnection) ConnectWireToNearestConnector(wire);
            }

            if (reconnectedWires.Count > 0) Debug.LogWarning(reconnectedWires.Count + " connectors have been reconnected to their wires");
        }

        private static void ReconnectWireToConnector(Wire wire, WireConnector connector)
        {
            //If this wire uses this connector, but it is not registered to it
            if (connector.wires.Contains(wire) == false)
            {
                //Check if it is atleast 30cm close
                float dist = Vector3.Distance(connector.transform.position, wire.transform.position);

                if (dist < 1f)
                {
                    //Reconnect wire
                    if (connector.wires.Contains(wire) == false)
                    {
                        connector.wires.Add(wire);

                        reconnectedWires.Add(wire);
                    }
                }
            }
        }

        private static void ConnectWireToNearestConnector(Wire wire)
        {
            foreach (WireConnectorGroup cluster in WireManager.Groups)
            {
                if (!cluster) return;

                foreach (WireConnector connector in cluster.connectors)
                {
                    if (!connector) return;

                    //Check if it is atleast 30cm close
                    float startDist = (connector.transform.position - wire.startPos).magnitude;
                    float endDist = (connector.transform.position - wire.endPos).magnitude;

                    if (startDist < 0.3f)
                    {
                        wire.startConnection = connector;
                        if (connector.wires.Contains(wire) == false)
                        {
                            connector.wires.Add(wire);

                            reconnectedWires.Add(wire);
                        }
                    }

                    if (endDist < 0.3f)
                    {
                        wire.endConnection = connector;
                        if (connector.wires.Contains(wire) == false)
                        {
                            connector.wires.Add(wire);

                            reconnectedWires.Add(wire);
                        }
                    }
                }
            }
        }
    }
}