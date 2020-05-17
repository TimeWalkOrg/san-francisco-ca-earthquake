/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainHUEWindow : Base.RealWorldTerrainBaseContainerTool
    {
        private int brightness;
        private int contrast;
        private int tone;
        private int saturation;

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
                            EditorUtility.DisplayProgressBar("Apply Brightness, Contrast and HUE", Mathf.RoundToInt(progress * 100) + "%", progress);
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
            if (brightness < 0) color = Color.Lerp(color, Color.black, (float)brightness / -150);
            else if (brightness > 0) color = Color.Lerp(color, Color.white, (float)brightness / 150);

            if (contrast != 0)
            {
                float c = Mathf.Pow((contrast + 100f) / 100, 2);

                float r = (color.r - 0.5f) * c + 0.5f;
                float g = (color.g - 0.5f) * c + 0.5f;
                float b = (color.b - 0.5f) * c + 0.5f;

                color.r = Mathf.Clamp01(r);
                color.g = Mathf.Clamp01(g);
                color.b = Mathf.Clamp01(b);
            }

            if (tone != 0)
            {
                float t = tone >= 0 ? tone : tone + 360;
                float rt = t / 120;
                rt = rt - (int)rt;

                float r = color.r;
                float g = color.g;
                float b = color.b;

                float nr, ng, nb;

                if (t < 120)
                {
                    nr = Mathf.Lerp(r, b, rt);
                    ng = Mathf.Lerp(g, r, rt);
                    nb = Mathf.Lerp(b, g, rt);
                }
                else if (t < 240)
                {
                    nr = Mathf.Lerp(b, g, rt);
                    ng = Mathf.Lerp(r, b, rt);
                    nb = Mathf.Lerp(g, r, rt);
                }
                else
                {
                    nr = Mathf.Lerp(g, r, rt);
                    ng = Mathf.Lerp(b, g, rt);
                    nb = Mathf.Lerp(r, b, rt);
                }

                color.r = nr;
                color.g = ng;
                color.b = nb;
            }

            if (saturation != 0)
            {
                float s = Mathf.Pow((saturation + 100f) / 100, 2);
                float averageColor = color.grayscale;
                float difR = color.r - averageColor;
                float difG = color.g - averageColor;
                float difB = color.b - averageColor;
                color.r = averageColor + difR * s;
                color.g = averageColor + difG * s;
                color.b = averageColor + difB * s;
            }

            return color;
        }

        protected override void OnContentGUI()
        {
            EditorGUI.BeginChangeCheck();

            bool reset = false;

            EditorGUILayout.BeginHorizontal();
            brightness = EditorGUILayout.IntSlider("Brightness", brightness, -150, 150);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                brightness = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            contrast = EditorGUILayout.IntSlider("Contrast", contrast, -100, 100);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                contrast = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            tone = EditorGUILayout.IntSlider("Tone", tone, -180, 180);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                tone = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            saturation = EditorGUILayout.IntSlider("Saturation", saturation, -100, 100);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                saturation = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck() || reset) UpdatePreview();

            if (image != null && preview != null)
            {
                int width = (int)position.width;

                GUILayout.Box("", GUILayout.Height(width / 2), GUILayout.ExpandWidth(true));
                Rect lastRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type != EventType.Layout)
                {
                    float imgWidth = lastRect.width / 2 - 10;
                    EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 5, lastRect.y + 5, imgWidth, lastRect.height - 10), image, null, ScaleMode.ScaleToFit);
                    EditorGUI.DrawPreviewTexture(new Rect(lastRect.x + 5 + lastRect.width / 2, lastRect.y + 5, imgWidth, lastRect.height - 10), preview, null, ScaleMode.ScaleToFit);
                }
            }
        }

        protected override void UpdatePreview()
        {
            for (int i = 0; i < originalColors.Length; i++) previewColors[i] = ApplyFilters(originalColors[i]);

            preview.SetPixels(previewColors);
            preview.Apply();
        }

        public static void OpenWindow(RealWorldTerrainMonoBase item)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainHUEWindow>(true, "Brightness, Contrast and HUE", true);
            wnd.item = item;
            wnd.GetImage();
        }
    }
}