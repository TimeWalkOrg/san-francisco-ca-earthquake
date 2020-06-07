using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace WireBuilder
{
    public class WireGenerator
    {
        public static Wire New(WireType wireType)
        {
            GameObject newWire = new GameObject();
            newWire.name = wireType.name + " Wire";

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newWire, "Create wire");
#endif
            Wire wire = newWire.AddComponent<Wire>();

            wire.wireType = wireType;
            //Update with (new) wind data
            Update(wire, null, true);

            return wire;
        }

        public static float CalculateWireSag(float gravity, float t)
        {
            return gravity * -Mathf.Sin(t * Mathf.PI);
        }

        public static void Update(Wire wire, WireType type, bool updateWind = false)
        {
            if (!wire || type == null) return;

            wire.wireType = type;

            //Position in world-space between the two connectors
            wire.startPos = wire.startConnection ? wire.startConnection.transform.position : wire.transform.position;
            wire.endPos = wire.endConnection ? wire.endConnection.transform.position : wire.transform.position;
            wire.length = Vector3.Distance(wire.startPos, wire.endPos);

            //Physics
            wire.weight = wire.wireType.weight * 0.01f;
            wire.tension = wire.weight * wire.length;

            //Wire sagging
            float sagAmount = wire.tension + wire.weight + wire.sagOffset;
            float lowestPoint = CalculateWireSag(sagAmount, 0.5f);
            wire.sagDepth = lowestPoint;

            //Calculate number of points over length of wire
            int positionCount = Mathf.RoundToInt(wire.wireType.pointsPerMeter * wire.length + wire.sagDepth);
            positionCount = Mathf.Clamp(positionCount, 6, 50);
            wire.points = new Vector3[positionCount];

            //Set pivot point at lowest wire point in the center (world-space)
            Vector3 pivot = Vector3.Lerp(wire.startPos, wire.endPos, 0.5f);
            pivot.y += lowestPoint;
            wire.gameObject.transform.position = pivot;

            //Rotate forward axis in direction of wire
            Vector3 forward = (wire.endPos - wire.startPos).normalized;
            if (forward != Vector3.zero) wire.gameObject.transform.forward = forward;

            //Update positions of line renderer and such
            for (int i = 0; i < positionCount; i++)
            {
                //Sample point along wire length as 0-1 value
                float wireSamplePoint = (float)i / (float)(positionCount - 1);

                //Current position along wire
                Vector3 wirePoint = (wire.endPos - wire.startPos) * wireSamplePoint;

                //Offset at Y-axis by sagging amount
                wirePoint.y += CalculateWireSag(sagAmount, wireSamplePoint);

                //Transform position to local-space
                wire.points[i] = wire.transform.InverseTransformPoint(wire.startPos + wirePoint);
            }

            //Transform positions from local- to world-space
            wire.startPos = wire.transform.position + wire.points[0];
            wire.endPos = wire.transform.position + wire.points[wire.points.Length - 1];

            if (updateWind || wire.windData == null) wire.windData = NewWindData(wire);

            ValidateComponents(wire);

            if (wire.wireType.geometryType == WireType.GeometryType.Line)
            {
                UpdateLineRenderer(wire, updateWind);
            }
            if (wire.wireType.geometryType == WireType.GeometryType.Mesh)
            {
                UpdateMesh(wire, updateWind);
            }

            //Reapply tag and layer
            wire.gameObject.layer = wire.wireType.layer;
            wire.gameObject.tag = wire.wireType.tag;

#if VEGETATION_STUDIO || VEGETATION_STUDIO_PRO
            wire.UpdateVegetationMask(wire.wireType);
#endif

#if UNITY_EDITOR
            EditorUtility.SetDirty(wire);
#endif
        }

        //Swap out components for Line and Mesh rendering
        private static void ValidateComponents(Wire wire)
        {
            if (wire.wireType.geometryType == WireType.GeometryType.Line)
            {
                if (wire.lineRenderer == false)
                {
                    wire.lineRenderer = wire.gameObject.AddComponent<LineRenderer>();
                    wire.lineRenderer.hideFlags = HideFlags.None;
                }

                if (wire.meshRenderer) GameObject.DestroyImmediate(wire.meshRenderer);
                if (wire.meshFilter) GameObject.DestroyImmediate(wire.meshFilter);

                wire.mesh = null;
            }

            if (wire.wireType.geometryType == WireType.GeometryType.Mesh)
            {
                if (wire.meshRenderer == false) wire.meshRenderer = wire.gameObject.AddComponent<MeshRenderer>();
                if (wire.meshFilter == false) wire.meshFilter = wire.gameObject.AddComponent<MeshFilter>();

                if (wire.lineRenderer) GameObject.DestroyImmediate(wire.lineRenderer);
            }
        }

        private static void UpdateLineRenderer(Wire wire, bool updateWind)
        {
            //lineRenderer.hideFlags = HideFlags.HideInInspector;
            wire.lineRenderer.useWorldSpace = false;
            wire.lineRenderer.generateLightingData = true;

            //Make wind updating optional to avoid wires jittering when updating (new random wind speed value)
            //Update wind gradient appears to be opague at edge
            if (wire.lineRenderer.colorGradient.alphaKeys[0].alpha == 1f || updateWind) wire.lineRenderer.colorGradient = NewWindData(wire);

            //Apply to line renderer
            wire.lineRenderer.positionCount = wire.points.Length;
            wire.lineRenderer.SetPositions(wire.points);

            //Line renders need double width to match a mesh wire
            wire.lineRenderer.startWidth = wire.wireType.diameter * 2f;
            wire.lineRenderer.endWidth = wire.wireType.diameter * 2f;

            wire.lineRenderer.material = wire.wireType.material;
            wire.lineRenderer.textureMode = wire.wireType.textureMode;

            wire.lineRenderer.lightmapScaleOffset = new Vector4(1f / wire.length, 1f, 0f, 0f);
        }

        private static void UpdateMesh(Wire wire, bool updateWind)
        {
            wire.meshRenderer.sharedMaterial = wire.wireType.material;
            wire.meshFilter.mesh = GenerateMesh(wire, updateWind);
        }

        //Store wind data in a gradient which is applied to the vertex colors over the length of the wire
        private static Gradient NewWindData(Wire wire)
        {
            Gradient g = new Gradient();

            float speedOffset = UnityEngine.Random.Range(0.2f, 1f);
            float weightOffset = UnityEngine.Random.Range(0.9f, 1f);

            //Red: Wind speed
            //Green: Wind weight
            //Blue: ...
            float speed = wire.tension * speedOffset;
            float weight = (weightOffset * Mathf.Abs(wire.sagDepth * 0.1f));
            Color color = new Color(speed, weight, 0f, 0f);

            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(color, 0f);
            colorKeys[1] = new GradientColorKey(color, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];
            for (int i = 0; i < 8; ++i)
            {
                var time = i / (float)(8 - 1);
                alphaKeys[i] = new GradientAlphaKey(Mathf.Sin(time * Mathf.PI), time);
            }

            g.SetKeys(colorKeys, alphaKeys);

            return g;
        }

        private static Mesh GenerateMesh(Wire wire, bool updateWind)
        {
            if (wire.mesh == null) wire.mesh = new Mesh();

            wire.mesh.name = wire.name + " (Mesh)";

            var verticesLength = wire.wireType.radialSegments * wire.points.Length;
            Vector3[] vertices = new Vector3[verticesLength];
            Color[] colors = new Color[verticesLength];

            int[] indices = GenerateIndices(wire);
            Vector2[] uvs = GenerateUVs(wire);
            colors = GenerateColors(wire);

            if (verticesLength > wire.mesh.vertexCount)
            {
                wire.mesh.vertices = vertices;
                wire.mesh.triangles = indices;
                wire.mesh.uv = uvs;
            }
            else
            {
                wire.mesh.triangles = indices;
                wire.mesh.vertices = vertices;
                wire.mesh.uv = uvs;
            }

            int currentVertIndex = 0;

            for (int i = 0; i < wire.points.Length; i++)
            {
                Vector3[] circle = VertexRing(i, wire);
                foreach (var vertex in circle)
                {
                    vertices[currentVertIndex++] = vertex;
                }
            }

            wire.mesh.vertices = vertices;
            wire.mesh.colors = colors;
            wire.mesh.RecalculateNormals();
            wire.mesh.RecalculateTangents();
            wire.mesh.RecalculateBounds();

            return wire.mesh;
        }

        private static int[] GenerateIndices(Wire wire)
        {
            // Two triangles and 3 vertices
            var indices = new int[wire.points.Length * wire.wireType.radialSegments * 2 * 3];

            var currentIndicesIndex = 0;
            for (int segment = 1; segment < wire.points.Length; segment++)
            {
                for (int side = 0; side < wire.wireType.radialSegments; side++)
                {
                    var vertIndex = (segment * wire.wireType.radialSegments + side);
                    var prevVertIndex = vertIndex - wire.wireType.radialSegments;

                    // Triangle one
                    indices[currentIndicesIndex++] = prevVertIndex;
                    indices[currentIndicesIndex++] = (side == wire.wireType.radialSegments - 1) ? (vertIndex - (wire.wireType.radialSegments - 1)) : (vertIndex + 1);
                    indices[currentIndicesIndex++] = vertIndex;

                    // Triangle two
                    indices[currentIndicesIndex++] = (side == wire.wireType.radialSegments - 1) ? (prevVertIndex - (wire.wireType.radialSegments - 1)) : (prevVertIndex + 1);
                    indices[currentIndicesIndex++] = (side == wire.wireType.radialSegments - 1) ? (vertIndex - (wire.wireType.radialSegments - 1)) : (vertIndex + 1);
                    indices[currentIndicesIndex++] = prevVertIndex;
                }
            }

            return indices;
        }

        private static Vector2[] GenerateUVs(Wire wire)
        {
            var uvs = new Vector2[wire.points.Length * wire.wireType.radialSegments];

            for (int segment = 0; segment < wire.points.Length; segment++)
            {
                for (int side = 0; side < wire.wireType.radialSegments; side++)
                {
                    int vertIndex = (segment * wire.wireType.radialSegments + side);
                    float u = side / (wire.wireType.radialSegments - 1f);
                    float v = (segment / (wire.points.Length - 1f)) * (wire.wireType.tiling * wire.length);

                    //Rotated 90 degrees
                    uvs[vertIndex] = new Vector2(v, u);
                }
            }

            return uvs;
        }

        private static Color[] GenerateColors(Wire wire)
        {
            Color[] colors = new Color[wire.points.Length * wire.wireType.radialSegments];

            float wireSamplePoint = 0;
            for (int segment = 0; segment < wire.points.Length; segment++)
            {
                //Sample point along wire length as 0-1 value
                wireSamplePoint = (float)segment / (float)(wire.points.Length - 1);

                for (int side = 0; side < wire.wireType.radialSegments; side++)
                {
                    int vertIndex = (segment * wire.wireType.radialSegments + side);

                    colors[vertIndex] = wire.windData.Evaluate(wireSamplePoint);
                }
            }

            return colors;
        }

        private static Vector3[] VertexRing(int index, Wire wire)
        {
            var dirCount = 0;
            var forward = Vector3.zero;

            //If not first index
            if (index > 0)
            {
                forward += (wire.points[index] - wire.points[index - 1]).normalized;
                dirCount++;
            }

            //If not last index
            if (index < wire.points.Length - 1)
            {
                forward += (wire.points[index + 1] - wire.points[index]).normalized;
                dirCount++;
            }

            //Forward is the average of the connecting edges directions
            forward = (forward / dirCount).normalized;
            var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
            var up = Vector3.Cross(forward, side).normalized;

            var circle = new Vector3[wire.wireType.radialSegments];
            var angle = 0f;
            var angleStep = (2 * Mathf.PI) / wire.wireType.radialSegments;

            for (int i = 0; i < wire.wireType.radialSegments; i++)
            {
                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);

                circle[i] = wire.points[index] + side * x * wire.wireType.diameter + up * y * wire.wireType.diameter;

                angle += angleStep;
            }

            return circle;
        }
    }
}