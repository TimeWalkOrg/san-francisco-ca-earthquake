/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateBuildingsPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Generate Buildings..."; }
        }

        public override void Enter()
        {
            try
            {
                if (prefs.buildingGenerator == 0)
                {
                    RealWorldTerrainBuildingGenerator.Generate(RealWorldTerrainWindow.container);
                }
                else if (prefs.buildingGenerator == 1)
                {
                    Debug.LogWarning("BuildR v1.x is no longer supported.");
                    phaseComplete = true;
                }
                else if (prefs.buildingGenerator == 2)
                {
                    RealWorldTerrainBuildR2Generator.Generate(RealWorldTerrainWindow.container);
                }
                else if (prefs.buildingGenerator == 3)
                {
                    RealWorldTerrainBuildingPrefabGenerator.Generate(RealWorldTerrainWindow.container);
                }
                else if (prefs.buildingGenerator == 4)
                {
                    RealWorldTerrainProceduralToolkitBuildingGenerator.Generate(RealWorldTerrainWindow.container);
                }

                progress = phaseProgress;
                if (phaseComplete) Complete();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                throw;
            }
        }
    }
}