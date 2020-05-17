/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainImporter : AssetPostprocessor
    {
        public static bool showMessage = true;
        public static bool fixImporterSettings = true;

        private void OnPreprocessTexture()
        {
            if (!fixImporterSettings) return;

            if (assetPath.Contains("RWT_Result"))
            {
                Match match = Regex.Match(assetPath, @"r(\d+)x(\d+)");
                if (match.Success)
                {
                    int width = int.Parse(match.Groups[1].Value);
                    int height = int.Parse(match.Groups[2].Value);
                    TextureImporter textureImporter = assetImporter as TextureImporter;
                    if (textureImporter == null) return;

                    if (showMessage)
                    {
                        bool needMessage = false;

                        if (!textureImporter.isReadable) needMessage = true;
                        if (textureImporter.mipmapEnabled) needMessage = true;
                        if (textureImporter.maxTextureSize != Mathf.Max(width, height)) needMessage = true;
                        if (textureImporter.wrapMode != TextureWrapMode.Clamp) needMessage = true;

                        if (needMessage && EditorUtility.DisplayDialog("Apply Settings?", "Settings are not optimal for development. Apply?", "Apply", "Revert")) return;
                    }

                    textureImporter.isReadable = true;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.maxTextureSize = Mathf.Max(width, height);
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                }
            }
        }
    }
}
