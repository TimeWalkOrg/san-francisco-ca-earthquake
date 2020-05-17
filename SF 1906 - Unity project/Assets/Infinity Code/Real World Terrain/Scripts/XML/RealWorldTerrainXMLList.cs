/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections;

#if !NETFX_CORE
using System.Xml;
#else
using Windows.Data.Xml.Dom;
#endif

namespace InfinityCode.RealWorldTerrain.XML
{
    /// <summary>
    /// Wrapper for XmlNodeList.
    /// </summary>
    public class RealWorldTerrainXMLList : IEnumerable
    {
        private readonly XmlNodeList _list;

        /// <summary>
        /// Count of the elements.
        /// </summary>
        public int count
        {
            get { return _list != null ? _list.Count : 0; }
        }

        /// <summary>
        /// Reference to XmlNodeList.
        /// </summary>
        public XmlNodeList list
        {
            get { return _list; }
        }

        /// <summary>
        /// Create empty list.
        /// </summary>
        public RealWorldTerrainXMLList()
        {

        }

        /// <summary>
        /// Create wrapper for XmlNodeList.
        /// </summary>
        /// <param name="list">XmlNodeList.</param>
        public RealWorldTerrainXMLList(XmlNodeList list)
        {
            _list = list;
        }

        /// <summary>
        /// Get the element by index.
        /// </summary>
        /// <param name="index">Index of element.</param>
        /// <returns>Element.</returns>
        public RealWorldTerrainXML this[int index]
        {
            get
            {
                if (_list == null || index < 0 || index >= _list.Count) return new RealWorldTerrainXML();
                return new RealWorldTerrainXML(_list[index] as XmlElement);
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }
    }
}