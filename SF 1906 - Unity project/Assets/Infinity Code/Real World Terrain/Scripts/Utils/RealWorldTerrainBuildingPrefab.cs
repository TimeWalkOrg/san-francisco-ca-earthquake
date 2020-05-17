/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    [Serializable]
    public class RealWorldTerrainBuildingPrefab
    {
        public GameObject prefab;
        public List<OSMTag> tags;
        public SizeMode sizeMode = SizeMode.fitToBounds;
        public HeightMode heightMode = HeightMode.levelBased;
        public float fixedHeight = 15;

        public bool hasBounds
        {
            get { return prefab.GetComponent<Collider>() != null; }
        }

        [Serializable]
        public class OSMTag
        {
            public string key;
            public string value;

            public bool hasEmptyKey
            {
                get { return string.IsNullOrEmpty(key); }
            }

            public bool hasEmptyValue
            {
                get { return string.IsNullOrEmpty(value); }
            }

            public bool isEmpty
            {
                get { return hasEmptyKey && hasEmptyValue; }
            }
        }

        public enum SizeMode
        {
            originalSize,
            fitToBounds,
        }

        public enum HeightMode
        {
            original,
            averageXZ,
            levelBased,
            fixedHeight
        }
    }
}