/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainLoadHeightmapPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Load Elevations..."; }
        }

        public override void Enter()
        {
            try
            {
                if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM)
                {
                    for (index = 0; index < RealWorldTerrainElevationGenerator.elevations.Count; index++)
                    {
                        RealWorldTerrainElevationGenerator activeElevation = RealWorldTerrainElevationGenerator.elevations[index];
                        ((RealWorldTerrainSRTMElevationGenerator)activeElevation).ParseHeightmap();
                        if (!isCapturing) return;
                    }
                }
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.BingMaps)
                {
                    if (!RealWorldTerrainBingElevationGenerator.Load())
                    {
                        RealWorldTerrainWindow.CancelCapture();
                        Debug.LogError("Cannot load elevation map");
                        return;
                    }
                }
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.ArcGIS)
                {
                    if (!RealWorldTerrainArcGISElevationGenerator.Load())
                    {
                        RealWorldTerrainWindow.CancelCapture();
                        Debug.LogError("Cannot load elevation map");
                        return;
                    }
                }
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.Mapbox)
                {
                    if (!RealWorldTerrainMapboxElevationGenerator.Load())
                    {
                        RealWorldTerrainWindow.CancelCapture();
                        Debug.LogError("Cannot load elevation map");
                        return;
                    }
                }
                else if (prefs.elevationProvider == RealWorldTerrainElevationProvider.SRTM30)
                {
                    for (index = 0; index < RealWorldTerrainElevationGenerator.elevations.Count; index++)
                    {
                        RealWorldTerrainElevationGenerator activeElevation = RealWorldTerrainElevationGenerator.elevations[index];
                        ((RealWorldTerrainSRTM30ElevationGenerator)activeElevation).ParseHeightmap();
                        if (!isCapturing) return;
                    }
                }

                Complete();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message + "\n" + exception.StackTrace);
                RealWorldTerrainWindow.CancelCapture();
            }
        }
    }
}