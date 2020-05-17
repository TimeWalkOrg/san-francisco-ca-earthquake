/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class for OSM Relation.
    /// </summary>
    public class RealWorldTerrainOSMRelation : RealWorldTerrainOSMBase
    {
        /// <summary>
        /// List of relation members.
        /// </summary>
        public readonly List<RealWorldTerrainOSMRelationMember> members;

        public RealWorldTerrainOSMRelation(BinaryReader br)
        {
            id = br.ReadInt64().ToString();
            members = new List<RealWorldTerrainOSMRelationMember>();
            tags = new List<RealWorldTerrainOSMTag>();

            int memberCount = br.ReadInt32();
            for (int i = 0; i < memberCount; i++) members.Add(new RealWorldTerrainOSMRelationMember(br));
            int tagCount = br.ReadInt32();
            for (int i = 0; i < tagCount; i++) tags.Add(new RealWorldTerrainOSMTag(br));
        }

        public RealWorldTerrainOSMRelation(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            members = new List<RealWorldTerrainOSMRelationMember>();
            tags = new List<RealWorldTerrainOSMTag>();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (subNode.Name == "member") members.Add(new RealWorldTerrainOSMRelationMember(subNode));
                else if (subNode.Name == "tag") tags.Add(new RealWorldTerrainOSMTag(subNode));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(long.Parse(id));
            bw.Write(members.Count);
            foreach (RealWorldTerrainOSMRelationMember member in members) member.Write(bw);
            bw.Write(tags.Count);
            foreach (RealWorldTerrainOSMTag tag in tags) tag.Write(bw);
        }
    }
}