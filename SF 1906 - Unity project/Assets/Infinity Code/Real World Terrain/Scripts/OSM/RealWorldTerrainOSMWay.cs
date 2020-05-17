/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class for OSM Way.
    /// </summary>
    public class RealWorldTerrainOSMWay : RealWorldTerrainOSMBase
    {
        /// <summary>
        /// List of OSM Node ID.
        /// </summary>
        public List<string> nodeRefs;

        public List<RealWorldTerrainOSMWay> holes;

        public RealWorldTerrainOSMWay()
        {
        }

        public RealWorldTerrainOSMWay(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            nodeRefs = new List<string>();
            tags = new List<RealWorldTerrainOSMTag>();
            int refCount = br.ReadInt32();
            for (int i = 0; i < refCount; i++) nodeRefs.Add(br.ReadInt64().ToString());
            int tagCount = br.ReadInt32();
            for (int i = 0; i < tagCount; i++) tags.Add(new RealWorldTerrainOSMTag(br));
        }

        public RealWorldTerrainOSMWay(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            nodeRefs = new List<string>();
            tags = new List<RealWorldTerrainOSMTag>();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (subNode.Name == "nd") nodeRefs.Add(subNode.Attributes["ref"].Value);
                else if (subNode.Name == "tag") tags.Add(new RealWorldTerrainOSMTag(subNode));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(long.Parse(id));
            bw.Write(nodeRefs.Count);
            foreach (string nodeRef in nodeRefs) bw.Write(long.Parse(nodeRef));
            bw.Write(tags.Count);
            foreach (RealWorldTerrainOSMTag tag in tags) tag.Write(bw);
        }
    }
}