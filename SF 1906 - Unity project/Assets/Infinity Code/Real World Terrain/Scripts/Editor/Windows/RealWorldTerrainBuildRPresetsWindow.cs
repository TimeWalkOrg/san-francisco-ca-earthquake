/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainBuildRPresetsWindow : EditorWindow
    {
        public static string[] buildrGeneratorStyles;
        public static string[] buildrGeneratorTexturePacks;

        private static RealWorldTerrainBuildRPresetsWindow wnd;

        public static List<RealWorldTerrainBuildRPresetsItem> presets;
        private Vector2 scrollPosition;

        private static RealWorldTerrainPrefs prefs
        {
            get { return RealWorldTerrainWindow.prefs; }
        }

        public static void OpenWindow()
        {
            wnd = GetWindow<RealWorldTerrainBuildRPresetsWindow>("BuildR Presets");
            if (prefs.customBuildRPresets != null) presets = prefs.customBuildRPresets.ToList();
            else presets = new List<RealWorldTerrainBuildRPresetsItem>();
        }

        private static List<string> GetBuildRXML(string datatype)
        {
            const string xmlPath = "Assets/Buildr/XML/";
            string[] paths = Directory.GetFiles(xmlPath);
            List<string> filelist = new List<string>();

            foreach (string path in paths)
            {
                if (path.Contains(".meta")) continue;
                if (!path.Contains(".xml")) continue;

                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                XmlNodeList xmlData = xml.SelectNodes("data/datatype");

                if (xmlData != null && xmlData.Count > 0 && xmlData[0].FirstChild.Value == datatype) filelist.Add(path.Substring(xmlPath.Length));
            }

            return filelist;
        }

        private static string OnBuildRPreset(string val, string title)
        {
            GUILayout.BeginHorizontal();
            val = EditorGUILayout.TextField(title, val);
            if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
            {
                string v1 = EditorUtility.OpenFilePanel(title, Application.dataPath, "xml");
                if (!string.IsNullOrEmpty(v1)) val = v1;
            }
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) val = "";
            GUILayout.EndHorizontal();
            return val;
        }

        private void OnDestroy()
        {
            prefs.customBuildRPresets = presets.ToArray();
        }

        private void OnGUI()
        {
            if (buildrGeneratorStyles == null)
            {
                List<string> generatorStyles = GetBuildRXML("ProGen");
                buildrGeneratorStyles = generatorStyles.ToArray();
                prefs.customBuildRGeneratorStyle = generatorStyles.IndexOf("none.xml");
            }
            if (buildrGeneratorTexturePacks == null)
            {
                List<string> generatorTexturePacks = GetBuildRXML("TexturePack");
                buildrGeneratorTexturePacks = generatorTexturePacks.ToArray();
                prefs.customBuildRGeneratorTexturePack = generatorTexturePacks.IndexOf("alltextures.xml");
            }

            prefs.customBuildRGeneratorStyle = EditorGUILayout.Popup("Style: ", prefs.customBuildRGeneratorStyle, buildrGeneratorStyles);
            prefs.customBuildRGeneratorTexturePack = EditorGUILayout.Popup("Texture Pack: ", prefs.customBuildRGeneratorTexturePack, buildrGeneratorTexturePacks);

            if (presets == null)
            {
                if (prefs.customBuildRPresets != null) presets = prefs.customBuildRPresets.ToList();
                else presets = new List<RealWorldTerrainBuildRPresetsItem>();
            }

            if (GUILayout.Button("Add preset") || presets.Count == 0) presets.Add(new RealWorldTerrainBuildRPresetsItem());

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < presets.Count; i++)
            {
                RealWorldTerrainBuildRPresetsItem preset = presets[i];

                GUILayout.BeginHorizontal();

                GUILayout.Label((i + 1).ToString(), GUILayout.ExpandWidth(false));

                GUILayout.BeginVertical();

                preset.facade = OnBuildRPreset(preset.facade, "Facade presets:");
                preset.roof = OnBuildRPreset(preset.roof, "Roof presets:");
                preset.texture = OnBuildRPreset(preset.texture, "Texture presets:");

                GUILayout.EndVertical();

                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) presets[i] = null;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            presets.RemoveAll(p => p == null);

            if (GUILayout.Button("Close"))
            {
                wnd.Close();
            }
        }
    }
}
