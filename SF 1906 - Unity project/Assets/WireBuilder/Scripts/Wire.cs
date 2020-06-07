using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WireBuilder
{
    [ExecuteInEditMode]
    public class Wire : MonoBehaviour
    {
        public WireType wireType;

        public WireConnector startConnection;
        public WireConnector endConnection;

#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
        public AwesomeTechnologies.VegetationMaskLine vegetationMaskLine;
#endif

        public Vector3[] points;
        public Vector3 startPos;
        public Vector3 endPos;
        [Range(0f, 10f)]
        public float sagOffset = 0f;
        public float length;
        public float sagDepth;
        public float tension;
        public float weight;

        public Gradient windData;
        public LineRenderer lineRenderer;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public Mesh mesh;

        private void OnValidate()
        {
            //Wires should not be moved, no point in doing so
            this.transform.hideFlags = HideFlags.NotEditable;
        }

        private void OnEnable()
        {
            WireManager.AddWire(this);
        }

        private void OnDisable()
        {
            WireManager.RemoveWire(this);
        }

        public void UpdateWire(bool updateWind)
        {
            WireGenerator.Update(this, wireType, updateWind);
        }

        public void UpdateVegetationMask(WireType wireType)
        {
#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            bool enabled = wireType.enableTreeMask || wireType.enableLargeObjectMask;

            if (wireType.enableTreeMask == false && wireType.enableLargeObjectMask == false)
            {
                if (vegetationMaskLine)
                {
                    DestroyImmediate(vegetationMaskLine);
                    vegetationMaskLine = null;
                    return;
                }
            }
            else
            {
                if (!vegetationMaskLine)
                {
                    vegetationMaskLine = this.gameObject.AddComponent<AwesomeTechnologies.VegetationMaskLine>();
                }
            }

            if (!enabled) return;

            vegetationMaskLine.ShowHandles = false;
            vegetationMaskLine.ShowArea = false;

            vegetationMaskLine.LineWidth = 1;
            vegetationMaskLine.RemoveGrass = false;
            vegetationMaskLine.RemovePlants = false;
            vegetationMaskLine.RemoveObjects = false;

            vegetationMaskLine.RemoveTrees = wireType.enableTreeMask;
            vegetationMaskLine.AdditionalTreePerimiterMax = wireType.treeMaskWidth;
            vegetationMaskLine.AdditionalTreePerimiter = wireType.treeMaskWidth;

            vegetationMaskLine.RemoveLargeObjects = wireType.enableLargeObjectMask;
            vegetationMaskLine.AdditionalLargeObjectPerimiterMax = wireType.largeObjectMaskWidth;
            vegetationMaskLine.AdditionalObjectPerimiter = wireType.largeObjectMaskWidth;

            vegetationMaskLine.Nodes.Clear();
            vegetationMaskLine.Nodes = new List<AwesomeTechnologies.Node>();

            if (startConnection) vegetationMaskLine.AddNode(startConnection.transform.position);
            vegetationMaskLine.AddNode(this.transform.position);
            if (endConnection) vegetationMaskLine.AddNode(endConnection.transform.position);

            vegetationMaskLine.UpdateVegetationMask();
#endif
        }


    }
}