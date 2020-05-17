/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class is added to each created terrains.\n
    /// It contains all information about the terrain.
    /// </summary>
    [Serializable]
    [AddComponentMenu("")]
    public class RealWorldTerrainItem : RealWorldTerrainMonoBase
    {
        /// <summary>
        /// Reference to RealWorldTerrainContainer instance.
        /// </summary>
        public RealWorldTerrainContainer container;

        /// <summary>
        /// X index.
        /// </summary>
        public int x;

        /// <summary>
        /// Y index.
        /// </summary>
        public int y;

        /// <summary>
        /// Reversed Y index (ry = countY - y - 1).
        /// </summary>
        public int ry;

        public bool needUpdate = false;

        /// <summary>
        /// Reference to MeshFilter, if result is mesh.
        /// </summary>
        public MeshFilter meshFilter;

        /// <summary>
        /// Reference to Terrain, if result is terrain.
        /// </summary>
        public Terrain terrain;

        /// <summary>
        /// Refetence to texture
        /// </summary>
        public Texture2D texture
        {
            get
            {
                if (prefs.resultType == RealWorldTerrainResultType.terrain)
                {
#if !RTP

#if UNITY_2018_3_OR_NEWER
                    if (terrainData == null || terrainData.terrainLayers.Length == 0) return null;
                    TerrainLayer sp = terrainData.terrainLayers[0];
                    return sp != null? sp.diffuseTexture: null;
#else
                    if (terrainData == null || terrainData.splatPrototypes.Length == 0) return null;
                    SplatPrototype sp = terrainData.splatPrototypes[0];
                    return sp.texture;
#endif

#else
                    ReliefTerrain reliefTerrain = GetComponent<ReliefTerrain>();
                    if (reliefTerrain == null)
                    {

#if UNITY_2018_3_OR_NEWER
                        if (terrainData == null || terrainData.terrainLayers.Length == 0) return null;
                        TerrainLayer sp = terrainData.terrainLayers[0];
                        return sp.diffuseTexture;
#else
                        if (terrainData == null || terrainData.splatPrototypes.Length == 0) return null;
                        SplatPrototype sp = terrainData.splatPrototypes[0];
                        return sp.texture;

#endif
                    }
                    return reliefTerrain.ColorGlobal;
#endif
                }
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer == null) return null;
                }
                return meshRenderer.sharedMaterial != null? meshRenderer.sharedMaterial.mainTexture as Texture2D: null;
            }
            set
            {
                if (prefs.resultType == RealWorldTerrainResultType.terrain)
                {
#if !RTP
                    if (terrainData == null) return;

#if UNITY_2018_3_OR_NEWER
                    TerrainLayer[] tls = terrainData.terrainLayers;
                    if (tls.Length == 0) return;
                    if (tls[0] == null)
                    {
                        tls[0] = new TerrainLayer
                        {
                            tileSize = new Vector2(terrainData.size.x, terrainData.size.z)
                        };
#if UNITY_EDITOR
                        string path = container.folder + terrain.name + ".terrainlayer";
                        AssetDatabase.CreateAsset(tls[0], path);
                        AssetDatabase.Refresh();
                        tls[0] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
#endif
                    }
                    tls[0].diffuseTexture = value;
                    terrainData.terrainLayers = tls;
#else 
                    SplatPrototype[] splatPrototypes = terrainData.splatPrototypes;
                    if (splatPrototypes.Length == 0) return;
                    splatPrototypes[0].texture = value;
                    terrainData.splatPrototypes = splatPrototypes;

#endif

#else
                    ReliefTerrain reliefTerrain = GetComponent<ReliefTerrain>();
                    if (reliefTerrain == null) return;
                    reliefTerrain.ColorGlobal = value;
#endif
                }
                else
                {
                    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.sharedMaterial.mainTexture = value;
                    else
                    {
                        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                        {
                            mr.sharedMaterial.mainTexture = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets TerrainData.
        /// </summary>
        public TerrainData terrainData
        {
            get
            {
                return terrain != null? terrain.terrainData: null;
            }
        }

        public float GetHeightmapValueByMercator(double mx, double my)
        {
            float rx = (float) ((mx - leftMercator) / (rightMercator - leftMercator));
            float ry = (float) ((my - topMercator) / (bottomMercator - topMercator));
            return terrainData.GetInterpolatedHeight(rx, 1 - ry);
        }

        public override RealWorldTerrainItem GetItemByWorldPosition(Vector3 worldPosition)
        {
            return container.GetItemByWorldPosition(worldPosition);
        }

        public override bool GetWorldPosition(double lng, double lat, out Vector3 worldPosition)
        {
            worldPosition = new Vector3();

            if (!Contains(lng, lat))
            {
                Debug.Log("Wrong coordinates");
                return false;
            }

            Terrain tr = terrain;
            if (tr == null) return false;

            Bounds b = new Bounds(bounds.center + container.transform.position, bounds.size);

            double mx, my;
            RealWorldTerrainUtils.LatLongToMercat(lng, lat, out mx, out my);

            double lX = RealWorldTerrainUtils.Clamp((mx - leftMercator) / width, 0, 1);
            double lZ = RealWorldTerrainUtils.Clamp(1 - (my - topMercator) / height, 0, 1);

            double px = b.size.x * lX + b.min.x;
            double pz = b.size.z * lZ + b.min.z;

            TerrainData tData = terrainData;
            Vector3 tSize = tData.size;
            Vector3 position = transform.position;
            if (position.x > px || position.z > pz || position.x + tSize.x < px || position.z + tSize.z < pz)
            {
                return false;
            }

            double ix = (px - terrain.gameObject.transform.position.x) / tData.size.x;
            double iz = (pz - terrain.gameObject.transform.position.z) / tData.size.z;
            double py = tData.GetInterpolatedHeight((float)ix, (float)iz) + position.y;

            worldPosition.x = (float) px;
            worldPosition.y = (float) py;
            worldPosition.z = (float) pz;

            return true;
        }

        public override bool GetWorldPosition(Vector2 coordinates, out Vector3 worldPosition)
        {
            return GetWorldPosition(coordinates.x, coordinates.y, out worldPosition);
        }
    }
}