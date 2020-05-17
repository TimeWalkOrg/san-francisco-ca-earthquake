/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainScalerWindow : EditorWindow
    {
        private static RealWorldTerrainScalerWindow wnd;
        private RealWorldTerrainContainer item;
        private Vector3 scale = Vector3.one;
        private Vector3 size = Vector3.one;
        private SizeType sizeType = SizeType.relative;
#if !RTP
        private bool scaleFirstSP = true;
#endif

        private void OnGUI()
        {
            item = (RealWorldTerrainContainer)EditorGUILayout.ObjectField("Real World Terrain", item, typeof(RealWorldTerrainContainer), true);
            sizeType = (SizeType)EditorGUILayout.EnumPopup("Size Type", sizeType);
            if (sizeType == SizeType.relative) scale = EditorGUILayout.Vector3Field("Scale", scale);
            else size = EditorGUILayout.Vector3Field("Size", size);

            if (item.prefs.resultType == RealWorldTerrainResultType.terrain)
            {
#if !RTP
                scaleFirstSP = EditorGUILayout.Toggle("Scale first SplatPrototype", scaleFirstSP);
#endif
            }
            if (GUILayout.Button("Apply")) Scale();
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/Tools/Scaler")]
        public static void OpenWindow()
        {
            OpenWindow(null); 
        }

        public static void OpenWindow(RealWorldTerrainContainer item)
        {
            wnd = GetWindow<RealWorldTerrainScalerWindow>("Scaler", true);
            wnd.item = item;
            if (item != null) wnd.size = item.size;
        }

        private void Scale()
        {
            if (sizeType == SizeType.relative)
            {
                if (!ValidateValue(scale, "Scale")) return;
            }
            else
            {
                if (!ValidateValue(size, "Size")) return;
            }

            Vector3 center = item.bounds.center;
            Vector3 originalSize = item.bounds.size;
            Vector3 newSize = originalSize;
            Vector3 pscale;

            if (sizeType == SizeType.relative)
            {
                pscale = scale;
                center.Scale(pscale);
                newSize.Scale(pscale);
            }
            else
            {
                center = item.bounds.min + size / 2;
                newSize = size;
                pscale = new Vector3(newSize.x / originalSize.x, newSize.y / originalSize.y, newSize.z / originalSize.z);
            }

            item.bounds = new Bounds(center, newSize);
            item.size = newSize;

            if (item.prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                Vector3 localScale = item.transform.localScale;
                localScale.Scale(pscale);
                item.transform.localScale = localScale;
                item.size = newSize;
            }
            else
            {
                foreach (RealWorldTerrainItem terrain in item.terrains)
                {
                    Vector3 p = terrain.transform.position;
                    p.Scale(pscale);
                    terrain.transform.position = p;

                    Vector3 s = terrain.terrainData.size;
                    s.Scale(pscale);
                    terrain.terrainData.size = s;
                    terrain.size = s;
                    terrain.bounds = new Bounds(terrain.transform.position + s / 2, s);

#if !RTP
#if UNITY_2018_3_OR_NEWER
                    if (scaleFirstSP && terrain.terrainData.terrainLayers.Length > 0)
                    {
                        TerrainLayer[] tls = terrain.terrainData.terrainLayers;
                        TerrainLayer l = tls[0];
                        l.tileSize = new Vector2(l.tileSize.x * pscale.x, l.tileSize.y * pscale.z);
                        l.tileOffset = new Vector2(l.tileOffset.x * pscale.x, l.tileOffset.y * pscale.z);
                        l.diffuseTexture = l.diffuseTexture;
                        terrain.terrainData.terrainLayers = tls;
                    }
#else
                    if (scaleFirstSP && terrain.terrainData.splatPrototypes.Length > 0)
                    {
                        SplatPrototype[] sps = terrain.terrainData.splatPrototypes;
                        SplatPrototype sp = sps[0];
                        sp.tileSize = new Vector2(sp.tileSize.x * pscale.x, sp.tileSize.y * pscale.z);
                        sp.tileOffset = new Vector2(sp.tileOffset.x * pscale.x, sp.tileOffset.y * pscale.z);
                        sp.texture = sp.texture;
                        terrain.terrainData.splatPrototypes = sps;
                    }
#endif
#endif
                }
            }
            
            Close();
        }

        private bool ValidateValue(Vector3 value, string variableName)
        {
            if (Math.Abs(value.x) < float.Epsilon || Math.Abs(value.y) < float.Epsilon || Math.Abs(value.z) < float.Epsilon)
            {
                Debug.LogError(variableName + " failed!!! Value can not be zero.");
                return false;
            }
            if (scale.x < 0 || scale.y < 0 || scale.z < 0)
            {
                Debug.LogError(variableName + " failed!!! Value can not be lower zero.");
                return false;
            }
            return true;
        }

        public enum SizeType
        {
            absolute,
            relative
        }
    }
}