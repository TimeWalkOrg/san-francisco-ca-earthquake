/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

#if BUILDR2
using System.Collections.Generic;
using BuildR2;
#endif

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainBuildR2Material
    {
#if BUILDR2
        public Facade applyFacade;
        public Surface roofSurface;
        public Roof.Types roofType = Roof.Types.Flat;
        public List<Facade> facades;
#endif
    }
}