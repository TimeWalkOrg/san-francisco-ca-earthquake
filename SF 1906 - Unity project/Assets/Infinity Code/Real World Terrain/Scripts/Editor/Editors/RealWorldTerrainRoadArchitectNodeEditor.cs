/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEditor;

namespace InfinityCode.RealWorldTerrain.Editors
{
    [CustomEditor(typeof(RealWorldTerrainRoadArchitectNode))]
    public class RealWorldTerrainRoadArchitectNodeEditor : Editor
    {
#if ROADARCHITECT
        private RealWorldTerrainRoadArchitectNode node;
        private GSDSplineN splineN;

        private void OnEnable()
        {
            node = (RealWorldTerrainRoadArchitectNode)target;
            splineN = node.GetComponent<GSDSplineN>();
        }

        public override void OnInspectorGUI()
        {
            if (splineN == null) return;

            bool allowIntersect = !splineN.bNeverIntersect;
            bool newAI = EditorGUILayout.Toggle("Allow intersect: ", allowIntersect);
            if (newAI != allowIntersect)
            {
                splineN.bNeverIntersect = !newAI;
            }
        }
#endif
    }
}