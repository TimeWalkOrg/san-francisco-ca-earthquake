/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

namespace InfinityCode.RealWorldTerrain.ExtraTypes
{
    /// <summary>
    /// The class contains the coordinates of the area boundaries.
    /// </summary>
    public class RealWorldTerrainGeoRect
    {
        /// <summary>
        /// Left longitude
        /// </summary>
        public double left;

        /// <summary>
        /// Right longitude
        /// </summary>
        public double right;

        /// <summary>
        /// Top latitude
        /// </summary>
        public double top;

        /// <summary>
        /// Bottom latitude
        /// </summary>
        public double bottom;

        /// <summary>
        /// Constructor
        /// </summary>
        public RealWorldTerrainGeoRect()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="left">Left longitude</param>
        /// <param name="top">Top latitude</param>
        /// <param name="right">Right longitude</param>
        /// <param name="bottom">Bottom latitude</param>
        public RealWorldTerrainGeoRect(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }
}