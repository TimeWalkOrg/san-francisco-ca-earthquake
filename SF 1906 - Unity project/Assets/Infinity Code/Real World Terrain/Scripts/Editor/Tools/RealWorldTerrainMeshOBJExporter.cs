/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public static class RealWorldTerrainMeshOBJExporter
    {
        private static int index = 0;

        public static void Export(RealWorldTerrainMonoBase item)
        {
            string meshName = item.gameObject.name;
            string filename = EditorUtility.SaveFilePanel("Export .obj file", "", meshName, "obj");
            if (string.IsNullOrEmpty(filename)) return;

            StringBuilder meshString = new StringBuilder();

            meshString.Append("#").Append(meshName).Append(".obj")
                .Append("\n#").Append(System.DateTime.Now.ToLongDateString())
                .Append("\n#").Append(System.DateTime.Now.ToLongTimeString())
                .Append("\n#-------")
                .Append("\n\n");

            meshString.Append("g ").Append(meshName).Append("\n");

            MeshFilter[] filters = item.GetComponentsInChildren<MeshFilter>();
            index = 0;

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (EditorUtility.DisplayCancelableProgressBar("Export OBJ", "Please wait", i / (float) filters.Length))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                MeshToString(filter, meshString);
            }

            StreamWriter sw = new StreamWriter(filename);
            sw.Write(meshString.ToString());
            sw.Close();

            EditorUtility.ClearProgressBar();

            EditorUtility.RevealInFinder(filename);
        }

        private static void MeshToString(MeshFilter mf, StringBuilder meshString)
        {
            Transform t = mf.transform;
            Quaternion r = t.localRotation;

            meshString.Append("g ").Append(t.name).Append("\n");

            int numVertices = 0;
            Mesh m = mf.sharedMesh;
            Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = t.TransformPoint(vv);
                numVertices++;
                meshString.Append("v ").Append(v.x).Append(" ").Append(v.y).Append(" ").Append(-v.z).Append("\n");
            }
            meshString.Append("\n");
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = r * nn;
                meshString.Append("vn ").Append(-v.x).Append(" ").Append(-v.y).Append(" ").Append(v.z).Append("\n");
            }
            meshString.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                meshString.Append("vt ").Append(v.x).Append(" ").Append(v.y).Append("\n");
            }
            for (int material = 0; material < m.subMeshCount; material++)
            {
                meshString.Append("\n");
                meshString.Append("usemtl ").Append(mats[material].name).Append("\n");
                meshString.Append("usemap ").Append(mats[material].name).Append("\n");

                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int ti1 = triangles[i] + 1 + index;
                    int ti2 = triangles[i + 1] + 1 + index;
                    int ti3 = triangles[i + 2] + 1 + index;
                    meshString.Append("f ").Append(ti1).Append("/").Append(ti1).Append("/").Append(ti1).Append(" ").
                        Append(ti2).Append("/").Append(ti2).Append("/").Append(ti2).Append(" ").
                        Append(ti3).Append("/").Append(ti3).Append("/").Append(ti3).Append(" ").Append("\n");
                }
            }

            index += numVertices;
        }
    }
}
