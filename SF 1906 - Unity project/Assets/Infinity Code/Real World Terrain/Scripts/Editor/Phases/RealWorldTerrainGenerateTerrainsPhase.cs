/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.IO;
using System.Text.RegularExpressions;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Phases
{
    public class RealWorldTerrainGenerateTerrainsPhase : RealWorldTerrainPhase
    {
        public static RealWorldTerrainItem activeTerrainItem;
        private RealWorldTerrainContainer container;
        private double maxElevation;
        private double minElevation;
        private Vector3 size;
        private Vector3 scale;

        public override string title
        {
            get { return "Generate Terrains..."; }
        }

        private static RealWorldTerrainItem AddTerrainItem(int x, int y, double x1, double y1, double x2, double y2, Vector3 size, Vector3 scale, double maxElevation, double minElevation, GameObject GO)
        {
            RealWorldTerrainItem item = GO.AddComponent<RealWorldTerrainItem>();
            prefs.Apply(item);

            double tlx, tly, brx, bry;
            RealWorldTerrainUtils.MercatToLatLong(x1, y1, out tlx, out tly);
            RealWorldTerrainUtils.MercatToLatLong(x2, y2, out brx, out bry);

            item.SetCoordinates(x1, y1, x2, y2, tlx, tly, brx, bry);
            item.maxElevation = maxElevation;
            item.minElevation = minElevation;
            item.scale = scale;
            item.size = size;
            item.x = x;
            item.ry = prefs.terrainCount.y - y - 1;
            item.y = y;
            item.bounds = new Bounds(item.transform.position + size / 2, size);

            activeTerrainItem = item;

            return item;
        }

        private RealWorldTerrainItem CreateMesh(Transform parent, int x, int y, double x1, double y1, double x2, double y2, Vector3 size, Vector3 scale, double maxElevation, double minElevation)
        {
            GameObject GO = new GameObject();
            GO.transform.parent = parent;
            GO.transform.localPosition = new Vector3(size.x * x, 0, size.z * y);

            RealWorldTerrainItem item = AddTerrainItem(x, y, x1, y1, x2, y2, size, scale, maxElevation, minElevation, GO);
            GO.name = Regex.Replace(RealWorldTerrainPrefs.LoadPref("TerrainName", "Terrain {x}x{y}"), @"{\w+}", ReplaceTerrainToken);
            return item;
        }

        private RealWorldTerrainItem CreateTerrain(Transform parent, int x, int y, double x1, double y1, double x2, double y2, Vector3 size, Vector3 scale, double maxElevation, double minElevation)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = prefs.baseMapResolution,
                heightmapResolution = prefs.heightmapResolution,
                alphamapResolution = prefs.controlTextureResolution
            };
            tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
            tdata.size = size;

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.transform.parent = parent;
            GO.transform.localPosition = new Vector3(size.x * x, 0, size.z * y);
#if UNITY_2019_2_OR_NEWER
            GameObjectUtility.SetStaticEditorFlags(GO, GameObjectUtility.GetStaticEditorFlags(GO) & ~StaticEditorFlags.ContributeGI);
#else
            GameObjectUtility.SetStaticEditorFlags(GO, GameObjectUtility.GetStaticEditorFlags(GO) & ~StaticEditorFlags.LightmapStatic);
#endif

            RealWorldTerrainItem item = AddTerrainItem(x, y, x1, y1, x2, y2, size, scale, maxElevation, minElevation, GO);
            item.terrain = GO.GetComponent<Terrain>();
            GO.name = Regex.Replace(RealWorldTerrainPrefs.LoadPref("TerrainName", "Terrain {x}x{y}"), @"{\w+}", ReplaceTerrainToken);

            string filename = Path.Combine(container.folder, GO.name) + ".asset";
            AssetDatabase.CreateAsset(tdata, filename);
            AssetDatabase.SaveAssets();

            return item;
        }

        public override void Enter()
        {
            if (index >= prefs.terrainCount)
            {
                Complete();
                return;
            }

            RealWorldTerrainVector2i tCount = prefs.terrainCount;
            int x = index % tCount.x;
            int y = index / tCount.x;

            double fromX = prefs.leftLongitude;
            double fromY = prefs.topLatitude;
            double toX = prefs.rightLongitude;
            double toY = prefs.bottomLatitude;

            RealWorldTerrainUtils.LatLongToMercat(ref fromX, ref fromY);
            RealWorldTerrainUtils.LatLongToMercat(ref toX, ref toY);

            double tx1 = (toX - fromX) * (x / (double)tCount.x) + fromX;
            double ty1 = toY - (toY - fromY) * ((y + 1) / (double)tCount.y);
            double tx2 = (toX - fromX) * ((x + 1) / (double)tCount.x) + fromX;
            double ty2 = toY -  (toY - fromY) * (y / (double)tCount.y);

            int tIndex = y * tCount.x + x;
            progress = index / (float)tCount;

            if (prefs.resultType == RealWorldTerrainResultType.terrain)
            {
                terrains[x, y] = container.terrains[tIndex] = CreateTerrain(container.transform, x, y, tx1, ty1, tx2, ty2, size, scale, maxElevation, minElevation);
            }
            else if (prefs.resultType == RealWorldTerrainResultType.mesh)
            {
                terrains[x, y] = container.terrains[tIndex] = CreateMesh(container.transform, x, y, tx1, ty1, tx2, ty2, size, scale, maxElevation, minElevation);
            }

            container.terrains[tIndex].container = container;
            index++;
        }

        public override void Finish()
        {
            activeTerrainItem = null;
            container = null;
        }

        private string ReplaceResultToken(Match match)
        {
            string v = match.Value.ToLower().Trim('{', '}');
            if (v == "title") return prefs.title;
            if (v == "tllat") return prefs.topLatitude.ToString();
            if (v == "tllng") return prefs.leftLongitude.ToString();
            if (v == "brlat") return prefs.bottomLatitude.ToString();
            if (v == "brlng") return prefs.rightLongitude.ToString();
            if (v == "cx") return prefs.terrainCount.x.ToString();
            if (v == "cy") return prefs.terrainCount.y.ToString();
            if (v == "st") return prefs.sizeType.ToString();
            if (v == "me") return prefs.maxElevationType.ToString();
            if (v == "mu") return prefs.nodataValue.ToString();
            if (v == "ds") return prefs.depthSharpness.ToString();
            if (v == "dr") return prefs.detailResolution.ToString();
            if (v == "rpp") return prefs.resolutionPerPatch.ToString();
            if (v == "bmr") return prefs.baseMapResolution.ToString();
            if (v == "hmr") return prefs.heightmapResolution.ToString();
            if (v == "tp") return prefs.textureProvider.ToString();
            if (v == "tw") return prefs.textureSize.x.ToString();
            if (v == "th") return prefs.textureSize.y.ToString();
            if (v == "tml") return prefs.maxTextureLevel.ToString();
            if (v == "ticks") return DateTime.Now.Ticks.ToString();
            return v;
        }

        private string ReplaceTerrainToken(Match match)
        {
            string v = match.Value.ToLower().Trim('{', '}');
            if (v == "tllat") return activeTerrainItem.topLatitude.ToString();
            if (v == "tllng") return activeTerrainItem.leftLongitude.ToString();
            if (v == "brlat") return activeTerrainItem.bottomLatitude.ToString();
            if (v == "brlng") return activeTerrainItem.rightLongitude.ToString();
            if (v == "x") return activeTerrainItem.x.ToString();
            if (v == "y") return activeTerrainItem.ry.ToString();

            return v;
        }

        public override void Start()
        {
            string resultFolder = "Assets/RWT_Result";
            string resultFullPath = Path.Combine(Application.dataPath, "RWT_Result");
            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
            resultFolder += "/" + dateStr;
            resultFullPath = Path.Combine(resultFullPath, dateStr);
            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);
            else
            {
                int index = 1;
                while (true)
                {
                    string path = resultFullPath + "_" + index;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        resultFolder += "_" + index;
                        break;
                    }
                    index++;
                }
            }

            const float scaleYCoof = 1112.0f;
            const float baseScale = 1000;

            double fromX = prefs.leftLongitude;
            double fromY = prefs.topLatitude;
            double toX = prefs.rightLongitude;
            double toY = prefs.bottomLatitude;

            double rangeX = toX - fromX;
            double rangeY = fromY - toY;

            RealWorldTerrainVector2i tCount = prefs.terrainCount;

            double sizeX = 0;
            double sizeZ = 0;

            if (prefs.sizeType == 0 || prefs.sizeType == 2)
            {
                double scfY = Math.Sin(fromY * Mathf.Deg2Rad);
                double sctY = Math.Sin(toY * Mathf.Deg2Rad);
                double ccfY = Math.Cos(fromY * Mathf.Deg2Rad);
                double cctY = Math.Cos(toY * Mathf.Deg2Rad);
                double cX = Math.Cos(rangeX * Mathf.Deg2Rad);
                double sizeX1 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * scfY + ccfY * ccfY * cX));
                double sizeX2 = Math.Abs(RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(sctY * sctY + cctY * cctY * cX));
                sizeX = (sizeX1 + sizeX2) / 2.0;
                sizeZ = RealWorldTerrainUtils.EARTH_RADIUS * Math.Acos(scfY * sctY + ccfY * cctY);
            }
            else if (prefs.sizeType == 1)
            {
                sizeX = Math.Abs(rangeX / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
                sizeZ = Math.Abs(rangeY / 360 * RealWorldTerrainUtils.EQUATOR_LENGTH);
            }

            maxElevation = RealWorldTerrainUtils.MAX_ELEVATION;
            minElevation = -RealWorldTerrainUtils.MAX_ELEVATION;

            double sX = sizeX / tCount.x * baseScale * prefs.terrainScale.x;
            double sY;
            double sZ = sizeZ / tCount.y * baseScale * prefs.terrainScale.z;

            if (prefs.elevationRange == RealWorldTerrainElevationRange.autoDetect)
            {
                double maxEl, minEl;
                RealWorldTerrainElevationGenerator.GetElevationRange(out minEl, out maxEl);
                if (prefs.generateUnderWater && prefs.nodataValue != 0 && minEl > prefs.nodataValue) minEl = prefs.nodataValue;
                maxElevation = maxEl + prefs.autoDetectElevationOffset.y;
                minElevation = minEl - prefs.autoDetectElevationOffset.x;
            }
            else if (prefs.elevationRange == RealWorldTerrainElevationRange.fixedValue)
            {
                maxElevation = prefs.fixedMaxElevation;
                minElevation = prefs.fixedMinElevation;
            }

            sY = (maxElevation - minElevation) / scaleYCoof * baseScale * prefs.terrainScale.y;

            if (prefs.sizeType == 2)
            {
                double scaleX = sX / prefs.fixedTerrainSize.x;
                double scaleZ = sZ / prefs.fixedTerrainSize.z;
                sX = prefs.fixedTerrainSize.x;
                sZ = prefs.fixedTerrainSize.z;
                sY /= (scaleX + scaleZ) / 2;
            }

            sX = Math.Round(sX);
            sY = Math.Round(sY);
            sZ = Math.Round(sZ);

            if (sY < 1) sY = 1;

            RealWorldTerrainWindow.terrains = new RealWorldTerrainItem[tCount.x, tCount.y];
            scale = new Vector3((float)(sX * tCount.x / rangeX), (float)(sY / (maxElevation - minElevation)), (float)(sZ * tCount.y / rangeY));

            string baseName = Regex.Replace(RealWorldTerrainPrefs.LoadPref("ResultName", "RealWorld Terrain"), @"{\w+}", ReplaceResultToken);
            string containerName = baseName;

            if (RealWorldTerrainPrefs.LoadPref("AppendIndex", true))
            {
                int nameIndex = 0;

                while (GameObject.Find("/" + containerName))
                {
                    nameIndex++;
                    containerName = baseName + " " + nameIndex;
                }
            }

            size = new Vector3((float)sX, (float)sY, (float)sZ);

            container = RealWorldTerrainWindow.container = new GameObject(containerName).AddComponent<RealWorldTerrainContainer>();
            prefs.Apply(container);

            double mx1, my1, mx2, my2;
            RealWorldTerrainUtils.LatLongToMercat(fromX, fromY, out mx1, out my1);
            RealWorldTerrainUtils.LatLongToMercat(toX, toY, out mx2, out my2);

            container.SetCoordinates(mx1, my1, mx2, my2, fromX, fromY, toX, toY);
            container.folder = resultFolder;
            container.scale = scale;
            container.size = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);
            container.terrainCount = prefs.terrainCount;
            container.terrains = new RealWorldTerrainItem[prefs.terrainCount.x * prefs.terrainCount.y];
            container.minElevation = minElevation;
            container.maxElevation = maxElevation;
            container.title = prefs.title;
            container.bounds = new Bounds(container.size / 2, container.size);
            if (prefs.useAnchor)
            {
                RealWorldTerrainUtils.LatLongToMercat(prefs.anchorLongitude, prefs.anchorLatitude, out mx1, out my1);
                Vector3 p = RealWorldTerrainEditorUtils.CoordsToWorld(mx1, 0, my1, container);
                container.transform.position = -p;
            }
        }
    }
}