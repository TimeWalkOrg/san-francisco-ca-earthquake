/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// This class contains basic information about the building.
    /// </summary>
    [AddComponentMenu("")]
    public class RealWorldTerrainBuilding : MonoBehaviour
    {
        /// <summary>
        /// The height of the walls.
        /// </summary>
        public float baseHeight;

        /// <summary>
        /// Array of base vertices.
        /// </summary>
        public Vector3[] baseVerticles;

        /// <summary>
        /// Reference to RealWorldTerrainContainer instance.
        /// </summary>
        public RealWorldTerrainContainer container;

        /// <summary>
        /// Indicates that roof normals is inverted.
        /// </summary>
        public bool invertRoof;

        /// <summary>
        /// Indicates that walls normals is inverted.
        /// </summary>
        public bool invertWall;

        /// <summary>
        /// Reference to MeshFilter of roof.
        /// </summary>
        public MeshFilter roof;

        /// <summary>
        /// Height of roof.
        /// </summary>
        public float roofHeight;

        /// <summary>
        /// Type of roof.
        /// </summary>
        public RealWorldTerrainRoofType roofType;

        /// <summary>
        /// Reference to MeshFilter of wall.
        /// </summary>
        public MeshFilter wall;
    }
}