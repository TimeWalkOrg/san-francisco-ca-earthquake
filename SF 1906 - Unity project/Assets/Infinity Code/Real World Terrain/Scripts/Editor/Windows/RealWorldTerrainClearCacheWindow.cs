/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainClearCacheWindow : EditorWindow
    {
        private bool clearOSM = false;
        private bool clearSRTM = false;
        private bool clearTexture = false;
        private bool clearTextureErrorOnly = false;
        private bool clearHistory = false;
        private bool clearSettings = false;

        private long osmSize;
        private long srtmSize;
        private long textureSize;

        private void Clear()
        {
            if (clearSRTM) RealWorldTerrainUtils.SafeDeleteDirectory(RealWorldTerrainEditorUtils.heightmapCacheFolder);
            if (clearOSM) RealWorldTerrainUtils.SafeDeleteDirectory(RealWorldTerrainEditorUtils.osmCacheFolder);
            if (clearTexture)
            {
                if (!clearTextureErrorOnly)
                    RealWorldTerrainUtils.SafeDeleteDirectory(RealWorldTerrainEditorUtils.textureCacheFolder);
                else
                {
                    string[] files = Directory.GetFiles(RealWorldTerrainEditorUtils.textureCacheFolder, "*.err",
                        SearchOption.AllDirectories);
                    foreach (string file in files) RealWorldTerrainUtils.SafeDeleteFile(file);
                }
            }

            if (clearHistory)
            {
                RealWorldTerrainUtils.SafeDeleteDirectory(RealWorldTerrainEditorUtils.historyCacheFolder);
                RealWorldTerrainHistoryWindow.Load();
            }

            if (clearSettings)
            {
                if (File.Exists(RealWorldTerrainPrefs.prefsFilename)) File.Delete(RealWorldTerrainPrefs.prefsFilename);
                RealWorldTerrainSettingsWindow.ClearSettings();
                RealWorldTerrainEditorUtils.ClearFoldersCache();
            }

            EditorUtility.DisplayDialog("Complete", "Clear cache complete.", "OK");

            Close();
        }

        private void OnEnable()
        {
            osmSize = RealWorldTerrainUtils.GetDirectorySize(RealWorldTerrainEditorUtils.osmCacheFolder);
            srtmSize = RealWorldTerrainUtils.GetDirectorySize(RealWorldTerrainEditorUtils.heightmapCacheFolder);
            textureSize = RealWorldTerrainUtils.GetDirectorySize(RealWorldTerrainEditorUtils.textureCacheFolder);
        }

        public static string FormatSize(long size)
        {
            if (size > 10485760) return size / 1048576 + " MB";
            if (size > 1024) return (size / 1048576f).ToString("0.000") + " MB";
            return size + " B";
        }

        private void OnGUI()
        {
            clearSRTM = GUILayout.Toggle(clearSRTM, "Clear elevation cache (" + FormatSize(srtmSize) + ")");
            clearTexture = GUILayout.Toggle(clearTexture, "Clear texture cache (" + FormatSize(textureSize) + ")");
            if (clearTexture)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                clearTextureErrorOnly = GUILayout.Toggle(clearTextureErrorOnly, "Errors only");
                GUILayout.EndHorizontal();
            }
            clearOSM = GUILayout.Toggle(clearOSM, "Clear OSM cache (" + FormatSize(osmSize) + ")");
            clearHistory = GUILayout.Toggle(clearHistory, "Clear History");
            clearSettings = GUILayout.Toggle(clearSettings, "Clear Settings");

            if (GUILayout.Button("Open Cache Folder")) EditorUtility.RevealInFinder(RealWorldTerrainEditorUtils.cacheFolder);
            if (GUILayout.Button("Clear")) Clear();
        }

        public static void OpenWindow()
        {
            RealWorldTerrainClearCacheWindow wnd = GetWindow<RealWorldTerrainClearCacheWindow>(true, "Clear cache");
            DontDestroyOnLoad(wnd);
        }
    }
}