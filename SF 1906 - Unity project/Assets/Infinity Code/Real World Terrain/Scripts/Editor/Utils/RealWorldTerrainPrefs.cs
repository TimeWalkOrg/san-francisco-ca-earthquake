/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain
{
    public class RealWorldTerrainPrefs : RealWorldTerrainPrefsBase
    {
        public const string prefsFilename = "RealWorldTerrainPrefs.xml";

        private const string prefix = "RWT_";

        [IgnoreInXML]
        public bool allowChange = true;

        [IgnoreInXML]
        public Vector2 coordinatesFrom = new Vector2(-113.6438f, 36.0358f);

        [IgnoreInXML]
        public Vector2 coordinatesTo = new Vector2(-113.0670f, 35.5680f);

        public double leftLongitude = -113.6438;
        public double rightLongitude = -113.0670;
        public double topLatitude = 36.0358;
        public double bottomLatitude = 35.5680;

        public bool useAnchor;
        public double anchorLatitude;
        public double anchorLongitude;

        public bool generateBuildings;
        public bool generateGrass;
        public bool generateRivers;
        public bool generateRoads;
        public bool generateTextures = false;
        public bool generateTrees;
        public RealWorldTerrainVector2i terrainCount = RealWorldTerrainVector2i.one;

        [IgnoreInXML]
        private RealWorldTerrainTextureProviderManager.MapType _mapType;

        public RealWorldTerrainTextureProviderManager.MapType mapType
        {
            get
            {
                if (_mapType == null)
                {
                    _mapType = RealWorldTerrainTextureProviderManager.FindMapType(mapTypeID);
                    _mapType.LoadSettings(mapTypeExtraFields);
                }
                return _mapType;
            }
            set { _mapType = value; }
        }

        public void Apply(RealWorldTerrainMonoBase target)
        {
            target.prefs = new RealWorldTerrainPrefsBase();
            FieldInfo[] fieldInfos = typeof(RealWorldTerrainPrefsBase).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo info in fieldInfos) info.SetValue(target.prefs, info.GetValue(this));

            target.generatedBuildings = target.generatedBuildings || generateBuildings;
            target.generateGrass = target.generateGrass || generateGrass;
            target.generateRoads = target.generateRoads || generateRoads;
            target.generateTextures = generateTextures;
            target.generateTrees = target.generateTrees || generateTrees;
        }

        private void CreateChildNode(XmlNode node, string name, object value)
        {
            if (value == null) return;

            if (value is string) CreateNode(node, name, value as string, true);
            else if (value is bool || value is int || value is long || value is short || value is Enum) CreateNode(node, name, value);
            else if (value is float) CreateNode(node, name, ((float)value).ToString(RealWorldTerrainCultureInfo.numberFormat));
            else if (value is double) CreateNode(node, name, ((double)value).ToString(RealWorldTerrainCultureInfo.numberFormat));
            else if (value is UnityEngine.Object) CreateNode(node, name, AssetDatabase.GetAssetPath(value as UnityEngine.Object));
            else if (value is IEnumerable)
            {
                IEnumerable v = (IEnumerable)value;
                XmlNode n = CreateNode(node, name);
                foreach (var item in v) CreateChildNode(n, "Item", item);
            }
            else
            {
                XmlNode n = CreateNode(node, name);
                FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (FieldInfo field in fields) CreateChildNode(n, field.Name, field.GetValue(value));
            }
        }

        private XmlNode CreateNode(XmlNode node, string nodeName)
        {
            XmlDocument doc = node.OwnerDocument;
            if (doc == null) return null;
            XmlNode newNode = doc.CreateElement(nodeName);
            node.AppendChild(newNode);
            return newNode;
        }

        private XmlNode CreateNode(XmlNode node, string nodeName, object value)
        {
            if (value != null) return CreateNode(node, nodeName, value.ToString(), false);
            return null;
        }

        private XmlNode CreateNode(XmlNode node, string nodeName, string value, bool wrapCData)
        {
            XmlDocument doc = node.OwnerDocument;
            if (doc == null) return null;
            XmlNode newNode = doc.CreateElement(nodeName);
            if (!wrapCData) newNode.AppendChild(doc.CreateTextNode(value));
            else newNode.AppendChild(doc.CreateCDataSection(value));
            node.AppendChild(newNode);
            return newNode;
        }

        public static void DeletePref(string id)
        {
            EditorPrefs.DeleteKey(prefix + id);
        }

        public static RealWorldTerrainPrefs GetPrefs(RealWorldTerrainMonoBase item, bool isNew = false)
        {
            RealWorldTerrainPrefs prefs = new RealWorldTerrainPrefs
            {
                leftLongitude = item.leftLongitude,
                topLatitude = item.topLatitude,
                rightLongitude = item.rightLongitude,
                bottomLatitude = item.bottomLatitude,
                generateGrass = item.generateGrass,
                generateRoads = item.generateRoads,
                generateTextures = item.generateTextures,
                generateTrees = item.generateTrees,
            };

            FieldInfo[] fieldInfos = typeof(RealWorldTerrainPrefsBase).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo info in fieldInfos) info.SetValue(prefs, info.GetValue(item.prefs));

            if (item is RealWorldTerrainContainer) prefs.terrainCount = ((RealWorldTerrainContainer)item).terrainCount;
            else prefs.terrainCount = RealWorldTerrainVector2i.one;

            if (!isNew) prefs.allowChange = false;

            if (string.IsNullOrEmpty(prefs.mapTypeID)) prefs.mapTypeID = RealWorldTerrainTextureProviderManager.Upgrade(prefs.textureProvider);
            prefs.mapType = RealWorldTerrainTextureProviderManager.FindMapType(prefs.mapTypeID);
            prefs.mapType.LoadSettings(prefs.mapTypeExtraFields);

            return prefs;
        }

        public void Load()
        {
            if (!allowChange) return;

            LoadFromXML(prefsFilename);

            if (string.IsNullOrEmpty(mapTypeID)) mapTypeID = RealWorldTerrainTextureProviderManager.Upgrade(textureProvider);
            mapType = RealWorldTerrainTextureProviderManager.FindMapType(mapTypeID);
            mapType.LoadSettings(mapTypeExtraFields);
        }

        public void LoadField(FieldInfo field, object target, XmlNode node)
        {
            if (node == null) return;

            string value = node.InnerXml;
            if (string.IsNullOrEmpty(value)) return;

            Type type = field.FieldType;
            if (type == typeof(string)) field.SetValue(target, node.InnerText.Trim());
            else if (type.IsEnum)
            {
                try
                {
                    field.SetValue(target, Enum.Parse(type, value));
                }
                catch
                {
                }
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                     type == typeof(float) || type == typeof(double) ||
                     type == typeof(bool))

            {
                PropertyInfo[] properties = type.GetProperties();
                Type underlyingType = type;

                if (properties.Length == 2 && string.Equals(properties[0].Name, "HasValue", StringComparison.InvariantCultureIgnoreCase)) underlyingType = properties[1].PropertyType;

                try
                {
                    MethodInfo method = underlyingType.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
                    object obj;
                    if (method != null) obj = method.Invoke(null, new object[] {value, RealWorldTerrainCultureInfo.numberFormat});
                    else
                    {
                        method = underlyingType.GetMethod("Parse", new[] {typeof(string)});
                        obj = method.Invoke(null, new[] {value});
                    }

                    field.SetValue(target, obj);
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.Message + "\n" + exception.StackTrace);
                    throw;
                }
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                object v = AssetDatabase.LoadAssetAtPath(value, typeof(UnityEngine.Object));
                field.SetValue(target, v);
            }
            else if (type.IsArray)
            {
                Array v = Array.CreateInstance(type.GetElementType(), node.ChildNodes.Count);

                int index = 0;
                foreach (XmlNode itemNode in node.ChildNodes)
                {
                    Type elementType = type.GetElementType();

                    if (elementType == typeof(string))
                    {
                        v.SetValue(itemNode.FirstChild.Value, index);
                    }
                    else
                    {
                        object item = Activator.CreateInstance(elementType);

                        FieldInfo[] fields = elementType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (XmlNode fieldNode in itemNode.ChildNodes)
                        {
                            FieldInfo fieldInfo = fields.FirstOrDefault(f => f.Name == fieldNode.Name);
                            if (fieldInfo == null)
                            {
                                Debug.Log("No info for " + fieldNode.Name);
                                continue;
                            }

                            LoadField(fieldInfo, item, fieldNode);
                        }

                        v.SetValue(item, index);
                    }

                    index++;
                }

                field.SetValue(target, v);
            }
            else if (type.IsGenericType)
            {
                Type listType = type.GetGenericArguments()[0];
                object v = type.Assembly.CreateInstance(type.FullName);

                foreach (XmlNode itemNode in node.ChildNodes)
                {
                    object item = null;

                    if (listType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        item = AssetDatabase.LoadAssetAtPath(itemNode.FirstChild.InnerText, listType);
                        if (item == null) continue;
                    }
                    else if (listType.IsValueType)
                    {
                        try
                        {
                            MethodInfo method = listType.GetMethod("Parse", new[] {typeof(string), typeof(IFormatProvider)});
                            if (method != null) item = method.Invoke(null, new object[] { itemNode.FirstChild.InnerText, RealWorldTerrainCultureInfo.numberFormat});
                            else
                            {
                                method = listType.GetMethod("Parse", new[] {typeof(string)});
                                item = method.Invoke(null, new[] { itemNode.FirstChild.InnerText });
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        item = listType.Assembly.CreateInstance(listType.FullName);
                        FieldInfo[] fields = listType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (XmlNode fieldNode in itemNode.ChildNodes)
                        {
                            FieldInfo fieldInfo = fields.FirstOrDefault(f => f.Name == fieldNode.Name);
                            if (fieldInfo == null)
                            {
                                Debug.Log("No info for " + fieldNode.Name);
                                continue;
                            }

                            LoadField(fieldInfo, item, fieldNode);
                        }
                    }

                    try
                    {
                        type.GetMethod("Add").Invoke(v, new[] { item });
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(exception.Message + "\n" + exception.StackTrace);
                    }
                }

                field.SetValue(target, v);
            }
            else
            {
                try
                {
                    object v = type.Assembly.CreateInstance(type.FullName);
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        FieldInfo fieldInfo = fields.FirstOrDefault(f => f.Name == childNode.Name);
                        if (fieldInfo == null) continue;
                        LoadField(fieldInfo, v, childNode);
                    }
                    field.SetValue(target, v);
                }
                catch (Exception)
                {
                    Debug.Log(type.FullName);
                    Debug.Log(node.Name);
                    throw;
                }
            }
        }

        public void LoadFromXML(string filename)
        {
            if (!File.Exists(filename)) return;
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNode node = doc.FirstChild;
            Type type = GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                FieldInfo field = fields.FirstOrDefault(f => f.Name == childNode.Name);

                if (field == null)
                {
                    Debug.Log(childNode.Name);
                    Debug.Log("Cannot find field");
                    continue;
                }

                LoadField(field, this, childNode);

                if (field.Name == "coordinatesFrom")
                {
                    leftLongitude = coordinatesFrom.x;
                    topLatitude = coordinatesFrom.y;
                }
                else if (field.Name == "coordinatesTo")
                {
                    rightLongitude = coordinatesTo.x;
                    bottomLatitude = coordinatesTo.y;
                }
            }
        }

        public static bool LoadPref(string id, bool defVal)
        {
            string key = prefix + id;
            if (EditorPrefs.HasKey(key)) return EditorPrefs.GetBool(key);
            return defVal;
        }

        public static string LoadPref(string id, string defVal)
        {
            string key = prefix + id;
            if (EditorPrefs.HasKey(key)) return EditorPrefs.GetString(key);
            return defVal;
        }

        public void Save()
        {
            if (!allowChange) return;

            File.WriteAllText(prefsFilename, ToXML(new XmlDocument()).OuterXml, Encoding.UTF8);
            CultureInfo culture = CultureInfo.InvariantCulture;
            string coordsScript = "var Coords = {" + string.Format(RealWorldTerrainCultureInfo.numberFormat, "tlx: {0}, tly: {1}, brx: {2}, bry: {3}", leftLongitude, topLatitude, rightLongitude, bottomLatitude) + "};";

            if (POI != null)
            {
                coordsScript += "var POI = [";

                for (int i = 0; i < POI.Count; i++)
                {
                    RealWorldTerrainPOI poi = POI[i];
                    if (i > 0) coordsScript += ", ";
                    coordsScript += "{x: " + poi.x.ToString(RealWorldTerrainCultureInfo.numberFormat) + ", y:" + poi.y.ToString(RealWorldTerrainCultureInfo.numberFormat) + ", title: \"" + poi.title + "\"}";
                }

                coordsScript += "];";
            }

            string coordPath = Directory.GetFiles(Application.dataPath, "RWT_Coords.jscript", SearchOption.AllDirectories)[0].Replace('\\', '/');
            File.WriteAllText(coordPath, coordsScript);
        }

        public static void SetPref(string id, bool val)
        {
            EditorPrefs.SetBool(prefix + id, val);
        }

        public static void SetPref(string id, string val)
        {
            EditorPrefs.SetString(prefix + id, val);
        }

        public XmlNode ToXML(XmlDocument document)
        {
            XmlNode node = document.CreateElement("Prefs");

            mapTypeID = mapType.fullID;
            mapTypeExtraFields = mapType.GetSettings();

            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.IsDefined(typeof(IgnoreInXMLAttribute), false)) continue;
                try
                {
                    CreateChildNode(node, field.Name, field.GetValue(this));
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.Message + "\n" + exception.StackTrace);
                }
            }

            return node;
        }

        private class IgnoreInXMLAttribute : Attribute
        {
        }
    }
}