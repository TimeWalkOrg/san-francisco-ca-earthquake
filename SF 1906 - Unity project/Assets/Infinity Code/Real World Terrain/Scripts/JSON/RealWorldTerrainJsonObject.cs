/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using InfinityCode.RealWorldTerrain.Utils;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.JSON
{
    /// <summary>
    /// The wrapper for JSON dictonary.
    /// </summary>
    public class RealWorldTerrainJsonObject : RealWorldTerrainJsonItem
    {
        private Dictionary<string, RealWorldTerrainJsonItem> _table;

        /// <summary>
        /// Dictionary of items
        /// </summary>
        public Dictionary<string, RealWorldTerrainJsonItem> table
        {
            get { return _table; }
        }

        public override RealWorldTerrainJsonItem this[string key]
        {
            get { return Get(key); }
        }

        public override RealWorldTerrainJsonItem this[int index]
        {
            get
            {
                if (index < 0) return null;

                int i = 0;
                foreach (KeyValuePair<string, RealWorldTerrainJsonItem> pair in _table)
                {
                    if (i == index) return pair.Value;
                    i++;
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RealWorldTerrainJsonObject()
        {
            _table = new Dictionary<string, RealWorldTerrainJsonItem>();
        }

        /// <summary>
        /// Adds element to the dictionary
        /// </summary>
        /// <param name="name">Key</param>
        /// <param name="value">Value</param>
        public void Add(string name, RealWorldTerrainJsonItem value)
        {
            _table[name] = value;
        }

        public void Add(string name, object value)
        {
            if (value is string || value is bool || value is int || value is long || value is short || value is float || value is double) _table[name] = new RealWorldTerrainJsonValue(value);
            else if (value is UnityEngine.Object)
            {
                _table[name] = new RealWorldTerrainJsonValue((value as UnityEngine.Object).GetInstanceID());
            }
            else _table[name] = RealWorldTerrainJson.Serialize(value, BindingFlags.Instance | BindingFlags.Public);
        }

        public void Add(string name, object value, RealWorldTerrainJsonValue.ValueType valueType)
        {
            _table[name] = new RealWorldTerrainJsonValue(value, valueType);
        }

        public override RealWorldTerrainJsonItem AppendObject(object obj)
        {
            Combine(RealWorldTerrainJson.Serialize(obj));
            return this;
        }

        /// <summary>
        /// Combines two JSON Object.
        /// </summary>
        /// <param name="other">Other JSON Object</param>
        /// <param name="overwriteExistingValues">Overwrite the existing values?</param>
        public void Combine(RealWorldTerrainJsonItem other, bool overwriteExistingValues = false)
        {
            RealWorldTerrainJsonObject otherObj = other as RealWorldTerrainJsonObject;
            if (otherObj == null) throw new Exception("Only RealWorldTerrainJsonObject is allowed to be combined.");
            Dictionary<string, RealWorldTerrainJsonItem> otherDict = otherObj.table;
            foreach (KeyValuePair<string, RealWorldTerrainJsonItem> pair in otherDict)
            {
                if (overwriteExistingValues || !_table.ContainsKey(pair.Key)) _table[pair.Key] = pair.Value;
            }
        }

        public bool Contains(string key)
        {
            return _table.ContainsKey(key);
        }

        public RealWorldTerrainJsonArray CreateArray(string name)
        {
            RealWorldTerrainJsonArray array = new RealWorldTerrainJsonArray();
            Add(name, array);
            return array;
        }

        public RealWorldTerrainJsonObject CreateObject(string name)
        {
            RealWorldTerrainJsonObject obj = new RealWorldTerrainJsonObject();
            Add(name, obj);
            return obj;
        }

        public override object Deserialize(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            IEnumerable<MemberInfo> members = RealWorldTerrainReflectionHelper.GetMembers(type, bindingFlags);
            return Deserialize(type, members, bindingFlags);
        }

        /// <summary>
        /// Deserializes current element
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="members">Members of variable</param>
        /// <returns>Object</returns>
        public object Deserialize(Type type, IEnumerable<MemberInfo> members, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            object v = Activator.CreateInstance(type);
            DeserializeObject(v, members, bindingFlags);
            return v;
        }

        public void DeserializeObject(object obj, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            IEnumerable<MemberInfo> members = RealWorldTerrainReflectionHelper.GetMembers(obj.GetType(), bindingFlags);
            DeserializeObject(obj, members);
        }

        public void DeserializeObject(object obj, IEnumerable<MemberInfo> members, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            foreach (MemberInfo member in members)
            {
#if !NETFX_CORE
                MemberTypes memberType = member.MemberType;
                if (memberType != MemberTypes.Field && memberType != MemberTypes.Property) continue;
#else
                MemberTypes memberType;
                if (member is PropertyInfo) memberType = MemberTypes.Property;
                else if (member is FieldInfo) memberType = MemberTypes.Field;
                else continue;
#endif

                if (memberType == MemberTypes.Property && !((PropertyInfo) member).CanWrite) continue;
                RealWorldTerrainJsonItem item;

#if !NETFX_CORE
                object[] attributes = member.GetCustomAttributes(typeof(RealWorldTerrainJson.AliasAttribute), true);
                RealWorldTerrainJson.AliasAttribute alias = attributes.Length > 0 ? attributes[0] as RealWorldTerrainJson.AliasAttribute : null;
#else
                IEnumerable<Attribute> attributes = member.GetCustomAttributes(typeof(RealWorldTerrainJson.AliasAttribute), true);
                RealWorldTerrainJson.AliasAttribute alias = null;
                foreach (Attribute a in attributes)
                {
                    alias = a as RealWorldTerrainJson.AliasAttribute;
                    break;
                }
#endif
                if (alias == null || !alias.ignoreFieldName)
                {
                    if (_table.TryGetValue(member.Name, out item))
                    {
                        Type t = memberType == MemberTypes.Field ? ((FieldInfo) member).FieldType : ((PropertyInfo) member).PropertyType;
                        if (memberType == MemberTypes.Field) ((FieldInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags));
                        else ((PropertyInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags), null);
                        continue;
                    }
                }

                if (alias != null)
                {
                    for (int j = 0; j < alias.aliases.Length; j++)
                    {
                        if (_table.TryGetValue(alias.aliases[j], out item))
                        {
                            Type t = memberType == MemberTypes.Field ? ((FieldInfo) member).FieldType : ((PropertyInfo) member).PropertyType;
                            if (memberType == MemberTypes.Field) ((FieldInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags));
                            else ((PropertyInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags), null);
                            break;
                        }
                    }
                }
            }
        }

        private RealWorldTerrainJsonItem Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (key.Length > 2 && key[0] == '/' && key[1] == '/')
            {
                string k = key.Substring(2);
                if (string.IsNullOrEmpty(k) || k.StartsWith("//")) return null;
                return GetAll(k);
            }

            return GetThis(key);
        }

        private RealWorldTerrainJsonItem GetThis(string key)
        {
            RealWorldTerrainJsonItem item;
            int index = -1;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '/')
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                string k = key.Substring(0, index);
                if (!string.IsNullOrEmpty(k))
                {
                    if (_table.TryGetValue(k, out item))
                    {
                        string nextPart = key.Substring(index + 1);
                        return item[nextPart];
                    }
                }

                return null;
            }

            if (_table.TryGetValue(key, out item)) return item;
            return null;
        }

        public override RealWorldTerrainJsonItem GetAll(string k)
        {
            RealWorldTerrainJsonItem item = GetThis(k);
            RealWorldTerrainJsonArray arr = null;
            if (item != null)
            {
                arr = new RealWorldTerrainJsonArray();
                arr.Add(item);
            }

            var enumerator = _table.GetEnumerator();
            while (enumerator.MoveNext())
            {
                item = enumerator.Current.Value;
                RealWorldTerrainJsonArray subArr = item.GetAll(k) as RealWorldTerrainJsonArray;
                if (subArr != null)
                {
                    if (arr == null) arr = new RealWorldTerrainJsonArray();
                    arr.AddRange(subArr);
                }
            }

            return arr;
        }

        public override IEnumerator<RealWorldTerrainJsonItem> GetEnumerator()
        {
            return _table.Values.GetEnumerator();
        }

        /// <summary>
        /// Parse a string that contains JSON dictonary
        /// </summary>
        /// <param name="json">String that contains JSON dictonary</param>
        /// <returns>Instance</returns>
        public static RealWorldTerrainJsonObject ParseObject(string json)
        {
            return RealWorldTerrainJson.Parse(json) as RealWorldTerrainJsonObject;
        }

        public RealWorldTerrainJsonItem Remove(string key)
        {
            RealWorldTerrainJsonItem item;
            if (_table.TryGetValue(key, out item))
            {
                _table.Remove(key);
                return item;
            }

            return null;
        }

        public override void ToJSON(StringBuilder b)
        {
            b.Append("{");
            bool hasChilds = false;
            foreach (KeyValuePair<string, RealWorldTerrainJsonItem> pair in _table)
            {
                b.Append("\"").Append(pair.Key).Append("\"").Append(":");
                pair.Value.ToJSON(b);
                b.Append(",");
                hasChilds = true;
            }

            if (hasChilds) b.Remove(b.Length - 1, 1);
            b.Append("}");
        }

        public override object Value(Type type)
        {
            if (RealWorldTerrainReflectionHelper.IsValueType(type)) return Activator.CreateInstance(type);
            return Deserialize(type);
        }
    }
}