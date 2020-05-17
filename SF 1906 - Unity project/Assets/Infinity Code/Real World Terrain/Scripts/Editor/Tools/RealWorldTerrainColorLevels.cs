/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainColorLevels : Base.RealWorldTerrainBaseContainerTool
    {
        private Texture2D levels;
        private float minLevel = 0;
        private float maxLevel = 1;
        private float center = 0.5f;
        private float minInput = 0;
        private float maxInput = 1;

        protected override void Apply()
        {
            int tick = 0;

            int sx = countX;
            int sy = countY;

            for (int ty = 0; ty < sy; ty++)
            {
                for (int tx = 0; tx < sx; tx++)
                {
                    int tIndex = ty * sx + tx;

                    Texture2D texture = terrains[tIndex].texture;
                    if (texture == null) continue;
                    Color[] colors = texture.GetPixels();

                    int cl = colors.Length;

                    for (int i = 0; i < cl; i++)
                    {
                        tick++;

                        if (tick >= 1000)
                        {
                            float progress = ((ty * sx + tx) * cl + i) / (float)(sx * sy * cl);
                            EditorUtility.DisplayProgressBar("Apply Color Levels", Mathf.RoundToInt(progress * 100) + "%", progress);
                            tick = 0;
                        }

                        colors[i] = ApplyFilters(colors[i]);
                    }

                    Texture2D newTexture = new Texture2D(texture.width, texture.height);
                    newTexture.SetPixels(colors);
                    newTexture.Apply();

                    string path = AssetDatabase.GetAssetPath(texture);
                    File.WriteAllBytes(path, newTexture.EncodeToPNG());
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Close();
            EditorUtility.ClearProgressBar();
        }

        protected override Color ApplyFilters(Color color)
        {
            float g = color.grayscale;
            float ng = Mathf.Clamp01((g - minLevel) / (maxLevel - minLevel));
            ng = (ng + 0.5f) * (1.5f - center) - 0.5f;

            float scale = ng / g;
            color.r = (Mathf.Clamp01(color.r * scale) - minInput) * (maxInput - minInput);
            color.g = (Mathf.Clamp01(color.g * scale) - minInput) * (maxInput - minInput);
            color.b = (Mathf.Clamp01(color.b * scale) - minInput) * (maxInput - minInput);

            return color;
        }

        protected override void OnDestoyLate()
        {
            levels = null;
        }

        protected override void OnContentGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.Box(levels, GUILayout.Height(260), GUILayout.ExpandWidth(true));
            Rect lastRect = GUILayoutUtility.GetLastRect();

            if (Event.current.type != EventType.Layout)
            {
                EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 2, lastRect.y + 2, lastRect.width - 4, lastRect.height - 4), levels, null, ScaleMode.StretchToFill);
            }

            EditorGUILayout.MinMaxSlider(ref minLevel, ref maxLevel, 0, 1);
            center = EditorGUILayout.Slider(center, 0.01f, 0.99f);

            EditorGUILayout.MinMaxSlider(new GUIContent(string.Format("Output Colors ({0}-{1})", Mathf.RoundToInt(minInput * 256), Mathf.RoundToInt(maxInput * 256))), ref minInput, ref maxInput, 0, 1);

            if (EditorGUI.EndChangeCheck()) UpdatePreview();

            if (image != null && preview != null)
            {
                int width = (int)position.width;

                GUILayout.Box("", GUILayout.Height(width / 2), GUILayout.ExpandWidth(true));
                lastRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type != EventType.Layout)
                {
                    float imgWidth = lastRect.width / 2 - 10;
                    EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 5, lastRect.y + 5, imgWidth, lastRect.height - 10), image, null, ScaleMode.ScaleToFit);
                    EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 5 + lastRect.width / 2, lastRect.y + 5, imgWidth, lastRect.height - 10), preview, null, ScaleMode.ScaleToFit);
                }
            }
        }

        protected override void OnGetEmptyImage()
        {
            levels = null;
        }

        protected override void OnGetImageLate()
        {
            UpdateLevels();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase item)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainColorLevels>(true, "Color Levels", true);
            wnd.item = item;
            wnd.GetImage();
        }

        private void UpdateLevels()
        {
            int[] lR = new int[256];
            int[] lG = new int[256];
            int[] lB = new int[256];
            int[] lGr = new int[256];

            levels = new Texture2D(512, 256);
            for (int i = 0; i < originalColors.Length; i++)
            {
                Color32 clr = originalColors[i];
                lR[clr.r]++;
                lG[clr.g]++;
                lB[clr.b]++;
                lGr[Mathf.RoundToInt(originalColors[i].grayscale * 255)]++;
            }

            Color[] levelColors = new Color[512 * 256];
            for (int i = 0; i < 512 * 256; i++) levelColors[i] = Color.white;

            int maxValue = lGr.Max();
            float scale = 256f / maxValue;

            for (int x = 0; x < 256; x++)
            {
                int ty = Mathf.RoundToInt(lGr[x] * scale);
                for (int y = 0; y < ty; y++)
                {
                    levelColors[y * 512 + x * 2] = Color.black;
                    levelColors[y * 512 + x * 2 + 1] = Color.black;
                }
            }

            levels.SetPixels(levelColors);
            levels.Apply();
        }
    }
}