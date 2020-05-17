/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityCode.RealWorldTerrain.Generators;
using InfinityCode.RealWorldTerrain.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainEditorUtils
    {
        private static string _assetPath;
        private static string _cacheFolder;
        private static string _heightmapCacheFolder;
        private static string _historyCacheFolder;
        private static string _osmCacheFolder;
        private static string _textureCacheFolder;
        private static RealWorldTerrainItem lastC2WItem;

        public static string assetPath
        {
            get
            {
                if (_assetPath == null)
                {
                    string[] assets = AssetDatabase.FindAssets("RealWorldTerrainEditorUtils");
                    FileInfo info = new FileInfo(AssetDatabase.GUIDToAssetPath(assets[0]));
                    _assetPath = info.Directory.Parent.Parent.Parent.FullName.Substring(Application.dataPath.Length - 6) + "/";
                }
                return _assetPath;
            }
        }


        public static string cacheFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_cacheFolder))
                {
                    string cache = RealWorldTerrainPrefs.LoadPref("CacheFolder", "");
                    if (cache == "")
                    {
                        DirectoryInfo parent = new DirectoryInfo(Application.dataPath).Parent;
                        if (parent != null) _cacheFolder = Path.Combine(parent.ToString(), "RWT_Cache");
                    }
                    else _cacheFolder = cache;
                }
                if (!Directory.Exists(_cacheFolder)) Directory.CreateDirectory(_cacheFolder);
                return _cacheFolder;
            }
        }

        public static string heightmapCacheFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_heightmapCacheFolder))
                    _heightmapCacheFolder = Path.Combine(cacheFolder, "Heightmaps");
                if (!Directory.Exists(_heightmapCacheFolder)) Directory.CreateDirectory(_heightmapCacheFolder);
                return _heightmapCacheFolder;
            }
        }

        public static string historyCacheFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_historyCacheFolder))
                    _historyCacheFolder = Path.Combine(cacheFolder, "History");
                if (!Directory.Exists(_historyCacheFolder)) Directory.CreateDirectory(_historyCacheFolder);
                return _historyCacheFolder;
            }
        }

        public static string osmCacheFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_osmCacheFolder)) _osmCacheFolder = Path.Combine(cacheFolder, "OSM");
                if (!Directory.Exists(_osmCacheFolder)) Directory.CreateDirectory(_osmCacheFolder);
                return _osmCacheFolder;
            }
        }

        public static string textureCacheFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_textureCacheFolder)) _textureCacheFolder = Path.Combine(cacheFolder, "Textures");
                if (!Directory.Exists(_textureCacheFolder)) Directory.CreateDirectory(_textureCacheFolder);
                return _textureCacheFolder;
            }
        }

        public static void ClearFoldersCache()
        {
            _cacheFolder = String.Empty;
            _heightmapCacheFolder = String.Empty;
            _osmCacheFolder = String.Empty;
            _textureCacheFolder = String.Empty;
        }

        public static Vector3 CoordsToWorld(double mx, float y, double mz, RealWorldTerrainContainer globalContainer)
        {
            mx = (mx - globalContainer.leftMercator) / (globalContainer.rightMercator - globalContainer.leftMercator) * globalContainer.size.x;
            mz = (1 - (mz - globalContainer.topMercator) / (globalContainer.bottomMercator - globalContainer.topMercator)) * globalContainer.size.z;

            return new Vector3((float)mx, y * globalContainer.scale.y, (float)mz) + globalContainer.transform.position;
        }

        public static Vector3 CoordsToWorldWithElevation(Vector3 point, RealWorldTerrainContainer globalContainer, Vector3 offset = default(Vector3))
        {
            bool success;
            return CoordsToWorldWithElevation(point, globalContainer, offset, out success);
        }

        public static Vector3 CoordsToWorldWithElevation(Vector3 point, RealWorldTerrainContainer globalContainer, Vector3 offset, out bool success)
        {
            return CoordsToWorldWithElevation(point.x, point.z, point.y, globalContainer, offset, out success);
        }

        public static Vector3 CoordsToWorldWithElevation(double longitude, double latitude, float altitude, RealWorldTerrainContainer globalContainer, Vector3 offset, out bool success)
        {
            double mx, my;
            RealWorldTerrainUtils.LatLongToMercat(longitude, latitude, out mx, out my);
            Vector3 v = Vector3.zero;
            success = false;

            if (globalContainer.prefs.elevationType == RealWorldTerrainElevationType.realWorld)
            {
                double elevation = RealWorldTerrainElevationGenerator.GetElevation(mx, my);
                success = Math.Abs(elevation - double.MinValue) > double.Epsilon;
                if (!success) v = CoordsToWorld(mx, RealWorldTerrainWindow.prefs.nodataValue, my, globalContainer) - offset;
                else v = CoordsToWorld(mx, (float)(elevation - globalContainer.minElevation), my, globalContainer) - offset;
            }
            else if (globalContainer.prefs.elevationType == RealWorldTerrainElevationType.heightmap)
            {
                v = CoordsToWorld(mx, RealWorldTerrainWindow.prefs.nodataValue, my, globalContainer) - offset;

                if (lastC2WItem == null || !lastC2WItem.Contains(longitude, latitude))
                {
                    lastC2WItem = null;
                    for (int i = 0; i < globalContainer.terrains.Length; i++)
                    {
                        if (globalContainer.terrains[i].Contains(longitude, latitude))
                        {
                            lastC2WItem = globalContainer.terrains[i];
                            break;
                        }
                    }
                }

                if (lastC2WItem != null)
                {
                    v.y = lastC2WItem.GetHeightmapValueByMercator(mx, my);
                    success = true;
                }
            }

            return v;
        }

        public static Object FindAndLoad(string filename, Type type)
        {
#if !UNITY_WEBPLAYER
            string[] files = Directory.GetFiles("Assets", filename, SearchOption.AllDirectories);
            if (files.Length > 0) return AssetDatabase.LoadAssetAtPath(files[0], type);
#endif
            return null;
        }

        public static Material FindMaterial(string materialName)
        {
            try
            {
                string materialPath = "Assets" +
                                      Directory.GetFiles(Application.dataPath, materialName, SearchOption.AllDirectories)[0]
                                          .Substring(Application.dataPath.Length).Replace('\\', '/');
                return (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
            }
            catch
            {
                Debug.LogWarning("Can not find the material: " + materialName);
                return new Material(Shader.Find("Diffuse"));
            }
        }

        public static void GeneratePreviewTexture(TerrainData tdata, ref Texture2D previewTexture)
        {
            if (previewTexture != null) return;

            previewTexture = new Texture2D(1, 1);
            previewTexture.SetPixel(0, 0, Color.red);
            previewTexture.Apply();
#if UNITY_2018_3_OR_NEWER
            List<TerrainLayer> tls = tdata.terrainLayers.ToList();
            TerrainLayer previewTl = new TerrainLayer
            {
                diffuseTexture = previewTexture,
                tileSize = new Vector2(tdata.size.x, tdata.size.z)
            };
            tls.Add(previewTl);
            tdata.terrainLayers = tls.ToArray();
#else
            List<SplatPrototype> sps = tdata.splatPrototypes.ToList();
            SplatPrototype previewSp = new SplatPrototype
            {
                texture = previewTexture,
                tileSize = new Vector2(tdata.size.x, tdata.size.z)
            };
            sps.Add(previewSp);
            tdata.splatPrototypes = sps.ToArray();
#endif
        }

        public static void ImportPackage(string path, Warning warning = null, string errorMessage = null)
        {
            if (warning != null && !warning.Show()) return;
            if (string.IsNullOrEmpty(assetPath))
            {
                if (!string.IsNullOrEmpty(errorMessage)) Debug.LogError(errorMessage);
                return;
            }

            string filaname = assetPath + "\\" + path;
            if (!File.Exists(filaname))
            {
                if (!string.IsNullOrEmpty(errorMessage)) Debug.LogError(errorMessage);
                return;
            }

            AssetDatabase.ImportPackage(filaname, true);
        }

        public class Warning
        {
            public string title = "Warning";
            public string message;
            public string ok = "OK";
            public string cancel = "Cancel";

            public bool Show()
            {
                return EditorUtility.DisplayDialog(title, message, ok, cancel);
            }
        }

    }
}