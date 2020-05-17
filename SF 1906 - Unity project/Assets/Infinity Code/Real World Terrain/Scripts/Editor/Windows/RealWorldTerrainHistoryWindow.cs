/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Windows
{
    public class RealWorldTerrainHistoryWindow: EditorWindow
    {
        private const int maxRecentCount = 5;

        private static List<RealWorldTerrainHistoryItem> recent;
        private Vector2 scrollPosition;

        private static string historyPath
        {
            get
            {
                return Path.Combine(RealWorldTerrainEditorUtils.historyCacheFolder, "hystory.xml");
            }
        }

        public static void Add(RealWorldTerrainPrefs prefs)
        {
            string id = Guid.NewGuid().ToString();
            string path = Path.Combine(RealWorldTerrainEditorUtils.historyCacheFolder, id + ".xml");
            File.WriteAllText(path, prefs.ToXML(new XmlDocument()).OuterXml, Encoding.UTF8);

            if (recent == null) Load();
            recent.Add(new RealWorldTerrainHistoryItem(prefs, id));

            Save();
        }

        public static void Load()
        {
            if (recent == null) recent = new List<RealWorldTerrainHistoryItem>();
            else recent.Clear();

            if (!File.Exists(historyPath)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(historyPath);

            foreach (XmlNode node in doc.FirstChild.ChildNodes) recent.Add(new RealWorldTerrainHistoryItem(node));
        }

        private void OnGUI()
        {
            if (recent == null) Load();

            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("№", headerStyle, GUILayout.Width(40));
            GUILayout.Label("Title", headerStyle);
            GUILayout.Label("Time", headerStyle, GUILayout.Width(120));
            GUILayout.Box("", GUIStyle.none, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUIContent useContent = new GUIContent(">", "Restore");
            GUIContent deleteContent = new GUIContent("X", "Remove");

            int removeIndex = -1;
            int index = 1;
            for (int i = recent.Count - 1; i >= 0; i--)
            {
                RealWorldTerrainHistoryItem item = recent[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(index.ToString(), GUILayout.Width(40));
                GUILayout.Label(item.title);

                DateTime time = new DateTime(item.timestamp);

                GUILayout.Label(time.ToString("yyyy-MM-dd HH:mm"), GUILayout.Width(120));

                if (GUILayout.Button(useContent, GUILayout.Width(20)))
                {
                    if (RealWorldTerrainWindow.wnd != null) RealWorldTerrainWindow.wnd.Close();

                    RealWorldTerrainWindow.OpenWindow(RealWorldTerrainGenerateType.full, null);
                    RealWorldTerrainWindow.prefs.LoadFromXML(Path.Combine(RealWorldTerrainEditorUtils.historyCacheFolder, item.id + ".xml"));
                }

                if (GUILayout.Button(deleteContent, GUILayout.Width(20))) removeIndex = i;
                EditorGUILayout.EndHorizontal();
                index++;
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Close")) Close();

            if (removeIndex != -1)
            {
                RealWorldTerrainHistoryItem item = recent[removeIndex];
                string filaname = Path.Combine(RealWorldTerrainEditorUtils.historyCacheFolder, item.id + ".xml");
                if (File.Exists(filaname)) File.Delete(filaname);
                recent.RemoveAt(removeIndex);
                Save();
                Repaint();
            }
        }

        private static void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("History");
            doc.AppendChild(node);
            foreach (RealWorldTerrainHistoryItem item in recent) item.Save(node);
            File.WriteAllText(historyPath, doc.OuterXml, Encoding.UTF8);
        }

        [MenuItem("Window/Infinity Code/Real World Terrain/History")]
        public static void OpenWindow()
        {
            GetWindow<RealWorldTerrainHistoryWindow>(true, "History");
        }
    }

    public class RealWorldTerrainHistoryItem
    {
        public string title;
        public string id;
        public long timestamp;

        public RealWorldTerrainHistoryItem(RealWorldTerrainPrefs prefs, string id)
        {
            if (!string.IsNullOrEmpty(prefs.title)) title = prefs.title;
            else title = "(" + prefs.topLatitude + ", " + prefs.leftLongitude + ") - (" + prefs.bottomLatitude + ", " + prefs.rightLongitude + ")";

            this.id = id;
            timestamp = DateTime.Now.Ticks;
        }

        public RealWorldTerrainHistoryItem(XmlNode node)
        {
            title = node["Title"].InnerText.Trim();
            id = node["ID"].InnerText;
            timestamp = long.Parse(node["Timestamp"].InnerText);
        }

        public void Save(XmlNode node)
        {
            XmlDocument doc = node.OwnerDocument;
            XmlElement itemNode = doc.CreateElement("Item");
            XmlElement titleNode = doc.CreateElement("Title");
            XmlElement idNode = doc.CreateElement("ID");
            XmlElement timestampNode = doc.CreateElement("Timestamp");

            titleNode.AppendChild(doc.CreateCDataSection(title));
            idNode.AppendChild(doc.CreateTextNode(id));
            timestampNode.AppendChild(doc.CreateTextNode(timestamp.ToString()));

            itemNode.AppendChild(titleNode);
            itemNode.AppendChild(idNode);
            itemNode.AppendChild(timestampNode);
            node.AppendChild(itemNode);
        }
    }
}
