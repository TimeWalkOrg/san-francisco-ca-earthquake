/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class of meta-information (key / value).
    /// </summary>
    [Serializable]
    public class RealWorldTerrainOSMMetaTag
    {
        /// <summary>
        /// Tag value.
        /// </summary>
        public string info;

        /// <summary>
        /// Key title.
        /// </summary>
        public string title;
    }
}