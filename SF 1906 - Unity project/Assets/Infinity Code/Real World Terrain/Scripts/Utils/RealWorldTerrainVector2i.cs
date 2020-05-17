/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    /// <summary>
    /// Integer version of Vector2 struct.
    /// </summary>
    [Serializable]
    public struct RealWorldTerrainVector2i
    {
        /// <summary>
        /// Gets the RealWorldTerrainVector2i where x=1 and y=1.
        /// </summary>
        public static RealWorldTerrainVector2i one
        {
            get
            {
                return new RealWorldTerrainVector2i(1, 1);
            }
        }

        /// <summary>
        /// The x value.
        /// </summary>
        public int x;

        /// <summary>
        /// The y value.
        /// </summary>
        public int y;

        /// <summary>
        /// Returns the count items (X * Y).
        /// </summary>
        public int count
        {
            get { return x * y; }
        }

        /// <summary>
        /// Returns the maximum value of x and y.
        /// </summary>
        public int max
        {
            get { return Mathf.Max(x, y); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="X">X value.</param>
        /// <param name="Y">Y value.</param>
        public RealWorldTerrainVector2i(int X = 0, int Y = 0)
        {
            x = X;
            y = Y;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", x, y);
        }

        public static implicit operator Vector2(RealWorldTerrainVector2i val)
        {
            return new Vector2(val.x, val.y);
        }

        public static implicit operator int(RealWorldTerrainVector2i val)
        {
            return val.count;
        }

        public static RealWorldTerrainVector2i operator +(RealWorldTerrainVector2i v1, RealWorldTerrainVector2i v2)
        {
            return new RealWorldTerrainVector2i(v1.x + v2.x, v1.y + v2.y);
        }

        public static RealWorldTerrainVector2i operator -(RealWorldTerrainVector2i v1, RealWorldTerrainVector2i v2)
        {
            return new RealWorldTerrainVector2i(v1.x - v2.x, v1.y - v2.y);
        }
    }
}