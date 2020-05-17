/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using System.Xml;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// Class for OSM Relation member.
    /// </summary>
    public class RealWorldTerrainOSMRelationMember
    {
        /// <summary>
        /// ID of OSM Way.
        /// </summary>
        public readonly string reference;

        /// <summary>
        /// Role of member.
        /// </summary>
        public readonly string role;

        /// <summary>
        /// Type of member.
        /// </summary>
        public readonly string type;

        public RealWorldTerrainOSMRelationMember(BinaryReader br)
        {
            type = br.ReadString();
            reference = br.ReadInt64().ToString();
            role = br.ReadString();
        }

        public RealWorldTerrainOSMRelationMember(XmlNode node)
        {
            type = node.Attributes["type"].Value;
            reference = node.Attributes["ref"].Value;
            role = node.Attributes["role"].Value;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(type);
            bw.Write(long.Parse(reference));
            bw.Write(role);
        }
    }
}