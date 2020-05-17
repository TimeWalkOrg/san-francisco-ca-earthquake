/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Tools
{
    [Serializable]
    public class RealWorldTerrainColorItem
    {
        public bool deleted;

        private Color32 color;
        private bool expanded = true;
        private int rangeB = 10;
        private int rangeG = 255;
        private int rangeR = 10;

        public RealWorldTerrainColorItem()
        {
            color = Color.red;
        }

        public bool EqualWithRange(Color32 clr)
        {
            int fr = Mathf.Abs(clr.r - color.r);
            int fg = Mathf.Abs(clr.g - color.g);
            int fb = Mathf.Abs(clr.b - color.b);
            return fr < rangeR / 2f && fg < rangeG / 2f && fb < rangeB / 2f;
        }

        public XmlNode GetNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("Color");
            node.SetAttribute("color", RealWorldTerrainUtils.ColorToHex(color));
            node.SetAttribute("range", rangeR + "," + rangeG + "," + rangeB);
            return node;
        }

        public void OnGUI(int index)
        {
            GUILayout.BeginHorizontal();

            expanded = EditorGUILayout.Foldout(expanded, "Color " + index + ": ");

            color = EditorGUILayout.ColorField(color);

            if (GUILayout.Button(new GUIContent(RealWorldTerrainResources.deleteIcon, "Remove color"), GUIStyle.none,
                GUILayout.Width(24), GUILayout.Height(20)))
                deleted = true;

            GUILayout.EndHorizontal();

            if (expanded)
            {
                rangeR = EditorGUILayout.IntSlider("Range red: ", rangeR, 0, 255);
                rangeG = EditorGUILayout.IntSlider("Range green: ", rangeG, 0, 255);
                rangeB = EditorGUILayout.IntSlider("Range blue: ", rangeB, 0, 255);
            }
        }

        public void SetNode(XmlElement node)
        {
            color = RealWorldTerrainUtils.HexToColor(node.GetAttribute("color"));
            string range = node.GetAttribute("range");
            string[] rs = range.Split(',');
            rangeR = int.Parse(rs[0]);
            rangeG = int.Parse(rs[1]);
            rangeB = int.Parse(rs[2]);
        }
    }
}