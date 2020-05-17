/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Xml;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Class points of interest.
    /// </summary>
    [Serializable]
    public class RealWorldTerrainPOI
    {
        /// <summary>
        /// The title of the POI.
        /// </summary>
        public string title;

        /// <summary>
        /// Longitude.
        /// </summary>
        public double x;

        /// <summary>
        /// Latitude.
        /// </summary>
        public double y;

        /// <summary>
        /// Altitude
        /// </summary>
        public float altitude;

        public GameObject prefab;

        public RealWorldTerrainPOI()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">POI title.</param>
        /// <param name="x">Longitude.</param>
        /// <param name="y">Latitude.</param>
        public RealWorldTerrainPOI(string title, double x, double y, float altitude = 0)
        {
            this.title = title;
            this.x = x;
            this.y = y;
            this.altitude = altitude;
        }

        public RealWorldTerrainPOI(XmlNode node)
        {
            try
            {
                x = RealWorldTerrainXMLExt.GetAttribute<double>(node, "x");
                y = RealWorldTerrainXMLExt.GetAttribute<double>(node, "y");
                title = node.InnerText;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }
    }
}