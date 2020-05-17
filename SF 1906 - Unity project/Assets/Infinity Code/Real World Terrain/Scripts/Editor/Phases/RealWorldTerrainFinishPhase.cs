/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainFinishPhase : RealWorldTerrainPhase
    {
        public override string title
        {
            get { return "Finish..."; }
        }

        private static void ApplyRTPTextures()
        {
#if RTP
            if ((RealWorldTerrainWindow.generateType != RealWorldTerrainGenerateType.full && RealWorldTerrainWindow.generateType != RealWorldTerrainGenerateType.texture) ||
                prefs.resultType != RealWorldTerrainResultType.terrain ||
                !prefs.generateTextures)
            {
                return;
            }

            ReliefTerrain reliefTerrain = terrains[0, 0].GetComponent<ReliefTerrain>();
            ReliefTerrainGlobalSettingsHolder settingsHolder = reliefTerrain.globalSettingsHolder;

            settingsHolder.numLayers = 4;
            settingsHolder.splats = new Texture2D[4];
            settingsHolder.Bumps = new Texture2D[4];
            settingsHolder.Heights = new Texture2D[4];

            for (int i = 0; i < 4; i++)
            {
                settingsHolder.splats[i] = RealWorldTerrainTextureGenerator.rtpTextures[i * 3];
                settingsHolder.Heights[i] = RealWorldTerrainTextureGenerator.rtpTextures[i * 3 + 1];
                settingsHolder.Bumps[i] = RealWorldTerrainTextureGenerator.rtpTextures[i * 3 + 2];
            }

            settingsHolder.GlobalColorMapBlendValues = new Vector3(1, 1, 1);
            settingsHolder._GlobalColorMapNearMIP = 1;
            settingsHolder.GlobalColorMapSaturation = 1;
            settingsHolder.GlobalColorMapSaturationFar = 1;
            settingsHolder.GlobalColorMapBrightness = 1;
            settingsHolder.GlobalColorMapBrightnessFar = 1;

            foreach (RealWorldTerrainItem item in terrains)
            {
#if !UNITY_5_2
                item.terrain.materialType = Terrain.MaterialType.Custom;
#endif
                item.GetComponent<ReliefTerrain>().RefreshTextures();
            }

            settingsHolder.Refresh();
#endif
        }

        public override void Complete()
        {
            phaseProgress = 0;
            phaseComplete = false;
            index = 0;
        }

        public override void Enter()
        {
            ApplyRTPTextures();
            GeneratePOI();

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                if (terrainCount > 1) GenerateNeighbors();
                foreach (RealWorldTerrainItem item in terrains) item.terrain.Flush();
            }
            else if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.terrain && 
                prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                foreach (RealWorldTerrainItem item in terrains)
                {
                    Texture2D texture = item["texture"] as Texture2D;
                    if (texture != null)
                    {
                        item.texture = texture;
                        item["texture"] = null;
                    }
                }
            }

            if (RealWorldTerrainWindow.generateType == RealWorldTerrainGenerateType.texture &&
                RealWorldTerrainWindow.generateTarget is RealWorldTerrainItem)
            {
                ((RealWorldTerrainItem)RealWorldTerrainWindow.generateTarget).needUpdate = true;
            }

            if (terrains != null)
            {
                foreach (RealWorldTerrainItem item in terrains)
                {
                    item.ClearCustomFields();
                }
            }

            if (RealWorldTerrainWindow.container != null) EditorGUIUtility.PingObject(RealWorldTerrainWindow.container);

            if (RealWorldTerrainWindow.OnCaptureCompleted != null) RealWorldTerrainWindow.OnCaptureCompleted();
            RealWorldTerrainWindow.Dispose();
            Complete();
        }

        private void GenerateNeighbors()
        {
            for (int x = 0; x < prefs.terrainCount.x; x++)
            {
                for (int y = 0; y < prefs.terrainCount.y; y++)
                {
                    Terrain bottom = y > 0 ? terrains[x, y - 1].terrain : null;
                    Terrain top = y < prefs.terrainCount.y - 1 ? terrains[x, y + 1].terrain : null;
                    Terrain left = x > 0 ? terrains[x - 1, y].terrain : null;
                    Terrain right = x < prefs.terrainCount.x - 1 ? terrains[x + 1, y].terrain : null;
                    terrains[x, y].terrain.SetNeighbors(left, top, right, bottom);
                }
            }
        }

        private static void GeneratePOI()
        {
            if (RealWorldTerrainWindow.generateType != RealWorldTerrainGenerateType.full || prefs.POI == null || prefs.POI.Count == 0) return;

            GameObject poiContainer = new GameObject("POI");
            poiContainer.transform.parent = RealWorldTerrainWindow.container.transform;

            foreach (RealWorldTerrainPOI poi in prefs.POI)
            {
                Vector3 pos;
                bool success;
                if (Math.Abs(poi.altitude) < float.Epsilon) pos = RealWorldTerrainEditorUtils.CoordsToWorldWithElevation(poi.x, poi.y, poi.altitude, RealWorldTerrainWindow.container, Vector3.zero, out success);
                else RealWorldTerrainWindow.container.GetWorldPosition(poi.x, poi.y, poi.altitude, out pos);

                GameObject poiGO;
                if (poi.prefab == null) poiGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                else poiGO = Object.Instantiate(poi.prefab);

                poiGO.AddComponent<RealWorldTerrainPOIItem>().SetPrefs(poi);
                poiGO.name = poi.title;
                poiGO.transform.parent = poiContainer.transform;
                poiGO.transform.position = pos;

                if (poi.prefab == null) poiGO.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            }
        }
    }
}