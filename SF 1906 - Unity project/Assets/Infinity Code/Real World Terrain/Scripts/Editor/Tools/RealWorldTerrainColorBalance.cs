/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    public class RealWorldTerrainColorBalance : Base.RealWorldTerrainBaseContainerTool
    {
        private float cyanRed;
        private float magentaGreen;
        private float yellowBlue;

        protected override void Apply()
        {
            int tick = 0;

            int sx = countX;
            int sy = countY;

            for (int ty = 0; ty < sy; ty++)
            {
                for (int tx = 0; tx < sx; tx++)
                {
                    int tIndex = ty * sy + tx;

                    Texture2D texture = terrains[tIndex].texture;
                    if (texture == null) continue;

                    Color[] colors = texture.GetPixels();

                    int cl = colors.Length;
                    for (int i = 0; i < cl; i++)
                    {
                        tick++;

                        if (tick >= 1000)
                        {
                            float progress = ((ty * sx + tx) * cl + i) / (float) (sx * sy * cl);
                            EditorUtility.DisplayProgressBar("Apply Color Ballance", Mathf.RoundToInt(progress * 100) + "%", progress);
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
            float r = 0;
            float g = 0;
            float b = 0;

            if (cyanRed > 0) r += cyanRed / 256;
            if (magentaGreen > 0) g += magentaGreen / 256;
            if (yellowBlue > 0) b += yellowBlue / 256;
            if (cyanRed < 0)
            {
                g += cyanRed / -512;
                b += cyanRed / -512;
            }
            if (magentaGreen < 0)
            {
                r += magentaGreen / -512;
                b += magentaGreen / -512;
            }
            if (yellowBlue < 0)
            {
                r += yellowBlue / -512;
                g += yellowBlue / -512;
            }

            color.r = Mathf.Clamp01(color.r + r);
            color.g = Mathf.Clamp01(color.g + g);
            color.b = Mathf.Clamp01(color.b + b);

            return color;
        }

        protected override void OnContentGUI()
        {
            EditorGUI.BeginChangeCheck();

            bool reset = false;

            EditorGUILayout.BeginHorizontal();
            cyanRed = EditorGUILayout.Slider("Cyan / Red", cyanRed, -100, 100);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                cyanRed = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            magentaGreen = EditorGUILayout.Slider("Magenta / Green", magentaGreen, -100, 100);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                magentaGreen = 0;
                reset = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            yellowBlue = EditorGUILayout.Slider("Yellow / Blue", yellowBlue, -100, 100);
            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
            {
                yellowBlue = 0;
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

        public static void OpenWindow(RealWorldTerrainMonoBase item)
        {
            if (wnd != null) wnd.Close();

            wnd = GetWindow<RealWorldTerrainColorBalance>(true, "Color Balance", true);
            wnd.item = item;
            wnd.GetImage();
        }

        protected override void UpdatePreview()
        {
            for (int i = 0; i < originalColors.Length; i++) previewColors[i] = ApplyFilters(originalColors[i]);

            preview.SetPixels(previewColors);
            preview.Apply();
        }
    }

}
