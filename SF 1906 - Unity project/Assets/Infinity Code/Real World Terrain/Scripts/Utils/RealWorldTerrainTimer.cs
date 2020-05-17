/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;

namespace InfinityCode.RealWorldTerrain
{
    public struct RealWorldTerrainTimer
    {
        private long start;

        public double seconds
        {
            get { return (DateTime.Now.Ticks - start) / 10000000d; }
        }

        public static RealWorldTerrainTimer Start()
        {
            return new RealWorldTerrainTimer
            {
                start = DateTime.Now.Ticks
            };
        }
    }
}
