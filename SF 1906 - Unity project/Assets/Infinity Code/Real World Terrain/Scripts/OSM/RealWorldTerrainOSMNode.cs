/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class for OSM Node.
    /// </summary>
    public class RealWorldTerrainOSMNode : RealWorldTerrainOSMBase
    {
        /// <summary>
        /// Latitude.
        /// </summary>
        public readonly float lat;

        /// <summary>
        /// Longitude.
        /// </summary>
        public readonly float lng;

        public int usageCount;

        public RealWorldTerrainOSMNode(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            lat = br.ReadSingle();
            lng = br.ReadSingle();

            tags = new List<RealWorldTerrainOSMTag>();
            int tagCount = br.ReadInt32();
            for (int i = 0; i < tagCount; i++) tags.Add(new RealWorldTerrainOSMTag(br));
        }

        public RealWorldTerrainOSMNode(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            lat = float.Parse(node.Attributes["lat"].Value, RealWorldTerrainCultureInfo.numberFormat);
            lng = float.Parse(node.Attributes["lon"].Value, RealWorldTerrainCultureInfo.numberFormat);

            tags = new List<RealWorldTerrainOSMTag>();

            foreach (XmlNode subNode in node.ChildNodes) tags.Add(new RealWorldTerrainOSMTag(subNode));
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(long.Parse(id));
            bw.Write(lat);
            bw.Write(lng);
            bw.Write(tags.Count);
            foreach (RealWorldTerrainOSMTag tag in tags) tag.Write(bw);
        }
    }
}