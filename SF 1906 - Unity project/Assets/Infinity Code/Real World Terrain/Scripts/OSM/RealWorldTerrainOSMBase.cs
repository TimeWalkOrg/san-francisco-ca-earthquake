/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.Linq;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// The base class for the Open Street Map objects.
    /// </summary>
    public class RealWorldTerrainOSMBase
    {
        /// <summary>
        /// ID.
        /// </summary>
        public string id;

        /// <summary>
        /// List of tags.
        /// </summary>
        public List<RealWorldTerrainOSMTag> tags;

        public bool Equals(RealWorldTerrainOSMBase other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        /// <summary>
        /// Gets tag value by key.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <returns>Tag value or string.Empty.</returns>
        public string GetTagValue(string key)
        {
            List<RealWorldTerrainOSMTag> curTags = tags.Where(tag => tag.key == key).ToList();
            if (curTags.Count > 0) return curTags[0].value;
            return string.Empty;
        }

        /// <summary>
        /// Checks tag with the specified pair (key, value).
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        /// <returns>True - success, False - otherwise.</returns>
        public bool HasTag(string key, string value)
        {
            return tags.Any(t => t.key == key && t.value == value);
        }

        /// <summary>
        /// Checks whether there is a tag with at least one of the keys.
        /// </summary>
        /// <param name="keys">Keys</param>
        /// <returns>True - success, False - otherwise.</returns>
        public bool HasTagKey(params string[] keys)
        {
            return keys.Any(key => tags.Any(t => t.key == key));
        }

        /// <summary>
        /// Checks whether there is a tag with at least one of the values.
        /// </summary>
        /// <param name="values">Values</param>
        /// <returns>True - success, False - otherwise.</returns>
        public bool HasTagValue(params string[] values)
        {
            return values.Any(val => tags.Any(t => t.value == val));
        }

        /// <summary>
        /// Checks whether there is a tag with key and at least one of the values.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="values">Values</param>
        /// <returns>True - success, False - otherwise.</returns>
        public bool HasTags(string key, params string[] values)
        {
            return tags.Any(tag => tag.key == key && values.Any(v => v == tag.value));
        }
    }
}