using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WireBuilder
{
    [ExecuteInEditMode]
    [AddComponentMenu("Wire Network/Connector group")]
    [HelpURL("http://staggart.xyz/unity/wire-builder/wb-docs/?section=components")]
    public class WireConnectorGroup : MonoBehaviour
    {
        public float handleSize = 1f;

        public List<WireConnector> connectors
        {
            get { return _connectors; }
            set { _connectors = value; }
        }
        [SerializeField]
        private List<WireConnector> _connectors = new List<WireConnector>();

        [ContextMenu("Refresh connectors")]
        private void UpdateConnectors()
        {
            foreach (WireConnector connector in connectors)
            {
                connector.group = this;
            }
        }

        private void OnEnable()
        {
            WireManager.AddConnectorGroup(this);
#if UNITY_EDITOR
            handleSize = TryGetMinBoundsSize();
#endif
        }

        private void OnDisable()
        {
            WireManager.RemoveConnectorGroup(this);
        }

        //Get the smallest size on all 3 axis
        private float TryGetMinBoundsSize()
        {
            MeshFilter mf = this.GetComponent<MeshFilter>();

            if (!mf)
            {
                return handleSize;
            }
            else
            {
                Bounds b = mf.sharedMesh.bounds;

                float min = 999;
                if (b.size.x < min) min = b.size.x;
                if (b.size.y < min) min = b.size.y;
                if (b.size.z < min) min = b.size.z;

                return min * 3f;
            }
        }
    }
}