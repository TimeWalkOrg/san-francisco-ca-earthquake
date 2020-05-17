/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.OSM
{
    /// <summary>
    /// This class contains meta-information about the building.
    /// </summary>
    [Serializable]
    [AddComponentMenu("")]
    public class RealWorldTerrainOSMMeta : MonoBehaviour
    {
        /// <summary>
        /// The coordinate of the center point of the building
        /// </summary>
        public Vector2 center;

        /// <summary>
        /// Indicates that building has a URL.
        /// </summary>
        public bool hasURL;

        /// <summary>
        /// Indicates that building has a website.
        /// </summary>
        public bool hasWebsite;

        /// <summary>
        /// Indicates that building has a Wikipedia page.
        /// </summary>
        public bool hasWikipedia;

        /// <summary>
        /// Array of meta-information.
        /// </summary>
        public RealWorldTerrainOSMMetaTag[] metaInfo;

        private void AddInfo(string title, string info)
        {
            if (metaInfo == null) metaInfo = new RealWorldTerrainOSMMetaTag[0];

            List<RealWorldTerrainOSMMetaTag> metaList = new List<RealWorldTerrainOSMMetaTag>(metaInfo)
            {
                new RealWorldTerrainOSMMetaTag {info = info, title = title}
            };

            if (title == "url") hasURL = true;
            else if (title == "website") hasWebsite = true;
            else if (title == "wikipedia") hasWikipedia = true;

            metaInfo = metaList.ToArray();
        }

        public RealWorldTerrainOSMMeta GetFromOSM(RealWorldTerrainOSMBase item, Vector2 center = default(Vector2))
        {
            foreach (RealWorldTerrainOSMTag itemTag in item.tags) AddInfo(itemTag.key, itemTag.value);
            this.center = center;

            return this;
        }
    }
}