/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Xml;
using InfinityCode.RealWorldTerrain.Tools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    [Serializable]
    public class RealWorldTerrainSplatPrototypeItem
    {
        public List<RealWorldTerrainColorItem> colors;
        public bool deleted;
        private readonly bool isBase;
        private bool expanded = true;
        private Texture2D texture;
        private Vector2 tileOffset = Vector2.zero;
        private Vector2 tileSize = new Vector2(15, 15);

        public SplatPrototype splat
        {
            get
            {
                return new SplatPrototype { texture = texture, tileSize = tileSize, tileOffset = tileOffset };
            }
        }

#if UNITY_2018_3_OR_NEWER
        public TerrainLayer terrainLayer
        {
            get
            {
                return new TerrainLayer { diffuseTexture = texture, tileSize = tileSize, tileOffset = tileOffset };
            }
        }
#endif

        public RealWorldTerrainSplatPrototypeItem(bool isBase = false)
        {
            this.isBase = isBase;
            colors = new List<RealWorldTerrainColorItem>();
        }

        public XmlNode GetNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("SplatPrototype");
            node.SetAttribute("tileSizeX", tileSize.x.ToString());
            node.SetAttribute("tileSizeY", tileSize.y.ToString());
            node.SetAttribute("tileOffsetX", tileOffset.x.ToString());
            node.SetAttribute("tileOffsetY", tileOffset.y.ToString());
            node.SetAttribute("textureID", (texture != null) ? texture.GetInstanceID().ToString() : "-1");

            foreach (RealWorldTerrainColorItem color in colors) node.AppendChild(color.GetNode(doc));

            return node;
        }

        public void OnGUI(int index = 0)
        {
            if (!isBase)
            {
                expanded = EditorGUILayout.Foldout(expanded, "SplatPrototype " + index);
                if (expanded)
                {
                    OnGUIProp("Texture: ");
                    int colorIndex = 1;
                    foreach (RealWorldTerrainColorItem color in colors) color.OnGUI(colorIndex++);
                    colors.RemoveAll(c => c.deleted);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Add color"))
                        colors.Add(new RealWorldTerrainColorItem());

                    if (GUILayout.Button("Generate preview"))
                        RealWorldTerrainSplatPrototypeGenerator.GeneratePreview(this);

                    if (GUILayout.Button("Remove SplatPrototype"))
                        deleted = true;

                    GUILayout.EndHorizontal();
                }
            }
            else
                OnGUIProp("Base texture: ");
        }

        private void OnGUIProp(string label)
        {
            texture = (Texture2D)EditorGUILayout.ObjectField(label, texture, typeof(Texture2D), false);
            tileSize = EditorGUILayout.Vector2Field("Tile size", tileSize);
            tileOffset = EditorGUILayout.Vector2Field("Tile offset", tileOffset);
        }

        public void SetNode(XmlElement node)
        {
            tileSize.x = float.Parse(node.GetAttribute("tileSizeX"));
            tileSize.y = float.Parse(node.GetAttribute("tileSizeY"));
            tileOffset.x = float.Parse(node.GetAttribute("tileOffsetX"));
            tileOffset.y = float.Parse(node.GetAttribute("tileOffsetY"));
            int textureID = int.Parse(node.GetAttribute("textureID"));
            if (textureID != -1) texture = (Texture2D)EditorUtility.InstanceIDToObject(textureID);
            else texture = null;

            colors = new List<RealWorldTerrainColorItem>();

            foreach (XmlElement cNode in node.ChildNodes)
            {
                RealWorldTerrainColorItem color = new RealWorldTerrainColorItem();
                color.SetNode(cNode);
                colors.Add(color);
            }
        }
    }
}