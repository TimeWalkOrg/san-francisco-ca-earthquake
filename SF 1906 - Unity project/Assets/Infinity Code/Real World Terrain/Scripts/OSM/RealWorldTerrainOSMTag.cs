/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using System.Xml;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class for OSM Tag.
    /// </summary>
    public class RealWorldTerrainOSMTag
    {
        /// <summary>
        /// Tag key.
        /// </summary>
        public readonly string key;

        /// <summary>
        /// Tag value.
        /// </summary>
        public readonly string value;

        public RealWorldTerrainOSMTag(BinaryReader br)
        {
            key = br.ReadString();
            value = br.ReadString();
        }

        public RealWorldTerrainOSMTag(XmlNode node)
        {
            key = node.Attributes["k"].Value;
            value = node.Attributes["v"].Value;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(key);
            bw.Write(value);
        }
    }
}