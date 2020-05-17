/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using InfinityCode.RealWorldTerrain.Net;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public abstract class RealWorldTerrainPhase
    {
        public static float phaseProgress;
        public static bool phaseComplete;
        public static int index;

        public static RealWorldTerrainPhase activePhase;
        public static int activePhaseIndex;
        public static List<RealWorldTerrainPhase> requiredPhases;

        protected static bool generateInThread
        {
            get { return RealWorldTerrainWindow.generateInThread; }
        }

        protected static bool isCapturing
        {
            get { return RealWorldTerrainWindow.isCapturing; }
            set { RealWorldTerrainWindow.isCapturing = value; }
        }

        protected static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static float progress
        {
            get { return RealWorldTerrainWindow.progress; }
            set
            {
                RealWorldTerrainWindow.progress = value;
                RealWorldTerrainWindow.wnd.Repaint();
            }
        }

        protected static RealWorldTerrainVector2i terrainCount
        {
            get { return prefs.terrainCount; }
        }

        protected static RealWorldTerrainItem[,] terrains
        {
            get { return RealWorldTerrainWindow.terrains; }
        }

        protected static RealWorldTerrainVector2i textureCount
        {
            get { return prefs.textureCount; }
        }

        public abstract string title { get; }

        public virtual void Complete()
        {
            phaseProgress = 0;
            phaseComplete = false;
            index = 0;

            NextPhase();
        }

        public abstract void Enter();

        public virtual void Finish()
        {
            
        }

        public static void Init()
        {
            requiredPhases = new List<RealWorldTerrainPhase>();
            activePhaseIndex = -1;

            if (RealWorldTerrainDownloadManager.count > 0) requiredPhases.Add(new RealWorldTerrainDownloadingPhase());

            bool gFull = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.full;
            bool gTerrain = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain;
            bool gTexture = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.texture;
            bool gAdditional = RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.additional;
            bool gGaia = gFull && prefs.resultType == RealWorldTerrainResultType.gaiaStamp;
            bool gRaw = gFull && prefs.resultType == RealWorldTerrainResultType.rawFile;
            bool isHeightmapOnly = gGaia || gRaw;

            bool nLoadHeightmap = !gTexture;
            bool nGenHeightmap = gFull || gTerrain;
            bool nTexture = (gFull || gTexture) && prefs.generateTextures;
            bool nAdditional = gFull || gAdditional;

            if (isHeightmapOnly)
            {
                nGenHeightmap = false;
                nTexture = false;
                nAdditional = false;
            }

            if (nLoadHeightmap)
            {
                requiredPhases.Add(new RealWorldTerrainUnzipHeightmapPhase());
                requiredPhases.Add(new RealWorldTerrainLoadHeightmapPhase());
            }

            if (!isHeightmapOnly)
            {
                if (gFull) requiredPhases.Add(new RealWorldTerrainGenerateTerrainsPhase());
                else requiredPhases.Add(new RealWorldTerrainLoadTerrainsPhase());
            }

            if (nGenHeightmap)
            {
                if (prefs.resultType == RealWorldTerrainResultType.terrain)
                {
                    requiredPhases.Add(new RealWorldTerrainGenerateHeightmapsPhase());
                    if (prefs.terrainCount > 1) requiredPhases.Add(new RealWorldTerrainAdjustEdgesPhase());
                }
                else if (prefs.resultType == RealWorldTerrainResultType.mesh)
                {
                    requiredPhases.Add(new RealWorldTerrainGenerateMeshVerticesPhase());
                    requiredPhases.Add(new RealWorldTerrainGenerateMeshesPhase());
                    requiredPhases.Add(new RealWorldTerrainInstantiateMeshesPhase());
                }
            }
#if GAIA_PRESENT
            if (gGaia) requiredPhases.Add(new RealWorldTerrainGenerateGaiaStampPhase());
#endif
            if (gRaw) requiredPhases.Add(new RealWorldTerrainGenerateRAWPhase());
            if (nTexture) requiredPhases.Add(new RealWorldTerrainGenerateTexturesPhase());

            if (nAdditional)
            {
                if (prefs.generateBuildings) requiredPhases.Add(new RealWorldTerrainGenerateBuildingsPhase());
                if (prefs.generateRoads) requiredPhases.Add(new RealWorldTerrainGenerateRoadsPhase());
                if (prefs.generateGrass) requiredPhases.Add(new RealWorldTerrainGenerateGrassPhase());
                if (prefs.generateTrees) requiredPhases.Add(new RealWorldTerrainGenerateTreesPhase());
                if (prefs.generateRivers) requiredPhases.Add(new RealWorldTerrainGenerateRiversPhase());
            }

            requiredPhases.Add(new RealWorldTerrainFinishPhase());
            activePhase = null;
            RealWorldTerrainImporter.showMessage = false;

            NextPhase();
        }

        public static void NextPhase()
        {
            if (activePhase != null) activePhase.Finish();

            activePhaseIndex++;
            if (requiredPhases == null || activePhaseIndex >= requiredPhases.Count)
            {
                Debug.Log("No active phase");
                activePhase = null;
                RealWorldTerrainWindow.Dispose();
                return;
            }

            activePhase = requiredPhases[activePhaseIndex];

            RealWorldTerrainGUIUtils.phasetitle = activePhase.title;
            progress = 0;
            RealWorldTerrainWindow.wnd.Repaint();

            activePhase.Start();
        }

        public virtual void Start()
        {

        }
    }
}