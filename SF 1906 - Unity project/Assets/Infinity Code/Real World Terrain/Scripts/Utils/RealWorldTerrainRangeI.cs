/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Class of integer range.
    /// </summary>
    [Serializable]
    public class RealWorldTerrainRangeI
    {
        /// <summary>
        /// Minimum value.
        /// </summary>
        public int min = 1;

        /// <summary>
        /// Minimum limit.
        /// </summary>
        public int minLimit = int.MinValue;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public int max = 50;

        /// <summary>
        /// Maximum limit.
        /// </summary>
        public int maxLimit = int.MaxValue;

        public RealWorldTerrainRangeI()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="minLimit">Minimum limit.</param>
        /// <param name="maxLimit">Maximum limit.</param>
        public RealWorldTerrainRangeI(int min, int max, int minLimit = int.MinValue, int maxLimit = int.MaxValue)
        {
            this.min = min;
            this.max = max;
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
        }

        /// <summary>
        /// Sets new minimum and maximum values.
        /// </summary>
        /// <param name="min">New minimum value.</param>
        /// <param name="max">New maximum value.</param>
        public void Set(float min, float max)
        {
            this.min = Mathf.Max(minLimit, (int)min);
            this.max = Mathf.Min(maxLimit, (int)max);
        }

        /// <summary>
        /// Gets random integer between the minimum and maximum.
        /// </summary>
        /// <returns>Rendom integer value.</returns>
        public int Random()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}