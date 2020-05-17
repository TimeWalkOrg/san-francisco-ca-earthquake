/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools.Base
{
    public class RealWorldTerrainBaseContainerTool:EditorWindow
    {
        protected static RealWorldTerrainBaseContainerTool wnd;

        public RealWorldTerrainMonoBase item;
        protected Texture2D image;
        protected Texture2D preview;
        protected Color[] originalColors;
        protected Color[] previewColors;

        private Vector2 scrollPosition;

        protected int countX
        {
            get
            {
                if (item == null) return 0;
                return item is RealWorldTerrainContainer ? (item as RealWorldTerrainContainer).terrainCount.x : 1;
            }
        }

        protected int countY
        {
            get
            {
                if (item == null) return 0;
                return item is RealWorldTerrainContainer ? (item as RealWorldTerrainContainer).terrainCount.y : 1;
            }
        }

        protected RealWorldTerrainItem[] terrains
        {
            get
            {
                if (item == null) return null;
                return item is RealWorldTerrainContainer ? (item as RealWorldTerrainContainer).terrains : new[] { item as RealWorldTerrainItem };
            }
        } 

        protected virtual void Apply()
        {
            
        }

        protected virtual Color ApplyFilters(Color color)
        {
            throw new Exception();
        }

        public void GetImage()
        {
            if (item == null)
            {
                image = null;
                preview = null;
                OnGetEmptyImage();
                return;
            }
            image = GetImage(item, 512);
            preview = new Texture2D(image.width, image.height);
            originalColors = image.GetPixels();
            previewColors = new Color[originalColors.Length];

            UpdatePreview();
            OnGetImageLate();
        }

        public static Texture2D GetImage(RealWorldTerrainMonoBase target, int imageWidth)
        {
            RealWorldTerrainContainer container = target as RealWorldTerrainContainer;
            RealWorldTerrainItem item = target as RealWorldTerrainItem;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            RealWorldTerrainItem[] terrains = container != null ? container.terrains : new[] { item };

            int imageHeight = Mathf.CeilToInt((float)imageWidth / cx * cy);
            Texture2D texture = new Texture2D(imageWidth, imageHeight);

            Color[] colors = new Color[imageWidth * imageHeight];

            for (int y = 0; y < imageHeight; y++)
            {
                float py = (float)y / imageHeight * cy;
                int ty = (int)py;
                float fy = py - ty;

                for (int x = 0; x < imageWidth; x++)
                {
                    float px = (float)x / imageWidth * cx;
                    int tx = (int)px;
                    float fx = px - tx;

                    int tIndex = ty * cx + tx;
                    Texture2D terrainTexture = terrains[tIndex].texture;
                    if (terrainTexture == null) continue;
                    colors[y * imageWidth + x] = terrainTexture.GetPixelBilinear(fx, fy);
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        protected virtual void OnGetImageLate()
        {
            
        }

        protected virtual void OnGetEmptyImage()
        {
            
        }

        protected virtual void OnContentGUI()
        {
            
        }

        private void OnDestroy()
        {
            wnd = null;
            item = null;
            image = null;
            preview = null;

            OnDestoyLate();
        }

        protected virtual void OnDestoyLate()
        {
            
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUI.BeginChangeCheck();
            item = EditorGUILayout.ObjectField("Target: ", item, typeof (RealWorldTerrainMonoBase), true) as RealWorldTerrainMonoBase;
            if (EditorGUI.EndChangeCheck()) GetImage();
            if (item == null) return;

            OnContentGUI();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply")) Apply();
        }

        protected virtual void UpdatePreview()
        {
            for (int i = 0; i < originalColors.Length; i++) previewColors[i] = ApplyFilters(originalColors[i]);

            preview.SetPixels(previewColors);
            preview.Apply();
        }
    }
}
