/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using InfinityCode.RealWorldTerrain.Utils;

namespace InfinityCode.RealWorldTerrain.JSON
{
    /// <summary>
    /// The wrapper for an array of JSON elements.
    /// </summary>
    public class RealWorldTerrainJsonArray : RealWorldTerrainJsonItem 
    {
        private List<RealWorldTerrainJsonItem> _items;
        private int _count;

        public List<RealWorldTerrainJsonItem> items
        {
            get { return _items; }
        }

        /// <summary>
        /// Count elements
        /// </summary>
        public int count
        {
            get { return _count; }
        }

        public override RealWorldTerrainJsonItem this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return null;
                return _items[index];
            }
        }


        public override RealWorldTerrainJsonItem this[string key]
        {
            get { return Get(key); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RealWorldTerrainJsonArray()
        {
            _items = new List<RealWorldTerrainJsonItem>();
        }

        /// <summary>
        /// Adds an element to the array.
        /// </summary>
        /// <param name="item">Element</param>
        public void Add(RealWorldTerrainJsonItem item)
        {
            _items.Add(item);
            _count++;
        }

        /// <summary>
        /// Adds an elements to the array.
        /// </summary>
        /// <param name="collection">Array of elements</param>
        public void AddRange(RealWorldTerrainJsonArray collection)
        {
            if (collection == null) return;
            _items.AddRange(collection._items);
            _count += collection._count;
        }

        public void AddRange(RealWorldTerrainJsonItem collection)
        {
            AddRange(collection as RealWorldTerrainJsonArray);
        }

        public RealWorldTerrainJsonObject CreateObject()
        {
            RealWorldTerrainJsonObject obj = new RealWorldTerrainJsonObject();
            Add(obj);
            return obj;
        }

        public override object Deserialize(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (_count == 0) return null;

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Array v = Array.CreateInstance(elementType, _count);
                if (_items[0] is RealWorldTerrainJsonObject)
                {
                    IEnumerable<MemberInfo> members = RealWorldTerrainReflectionHelper.GetMembers(elementType, bindingFlags);
                    for (int i = 0; i < _count; i++)
                    {
                        RealWorldTerrainJsonItem child = _items[i];
                        object item = (child as RealWorldTerrainJsonObject).Deserialize(elementType, members, bindingFlags);
                        v.SetValue(item, i);
                    }
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        RealWorldTerrainJsonItem child = _items[i];
                        object item = child.Deserialize(elementType, bindingFlags);
                        v.SetValue(item, i);
                    }
                }

                return v;
            }

            if (RealWorldTerrainReflectionHelper.IsGenericType(type))
            {
                Type listType = RealWorldTerrainReflectionHelper.GetGenericArguments(type)[0];
                object v = Activator.CreateInstance(type);

                if (_items[0] is RealWorldTerrainJsonObject)
                {
                    IEnumerable<MemberInfo> members = RealWorldTerrainReflectionHelper.GetMembers(listType, BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < _count; i++)
                    {
                        RealWorldTerrainJsonItem child = _items[i];
                        object item = (child as RealWorldTerrainJsonObject).Deserialize(listType, members);
                        try
                        {
                            MethodInfo methodInfo = RealWorldTerrainReflectionHelper.GetMethod(type, "Add");
                            if (methodInfo != null) methodInfo.Invoke(v, new[] { item });
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        RealWorldTerrainJsonItem child = _items[i];
                        object item = child.Deserialize(listType);
                        try
                        {
                            MethodInfo methodInfo = RealWorldTerrainReflectionHelper.GetMethod(type, "Add");
                            if (methodInfo != null) methodInfo.Invoke(v, new[] { item });
                        }
                        catch
                        {
                        }
                    }
                }

                return v;
            }


            return null;
        }

        private RealWorldTerrainJsonItem Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (key.StartsWith("//"))
            {
                string k = key.Substring(2);
                if (string.IsNullOrEmpty(k) || k.StartsWith("//")) return null;
                return GetAll(k);
            }

            return GetThis(key);
        }

        private RealWorldTerrainJsonItem GetThis(string key)
        {
            int kindex;

            if (key.Contains("/"))
            {
                int index = key.IndexOf("/");
                string k = key.Substring(0, index);
                string nextPart = key.Substring(index + 1);

                if (k == "*")
                {
                    RealWorldTerrainJsonArray arr = new RealWorldTerrainJsonArray();
                    for (int i = 0; i < _count; i++)
                    {
                        RealWorldTerrainJsonItem item = _items[i][nextPart];
                        if (item != null) arr.Add(item);
                    }

                    return arr;
                }

                if (int.TryParse(k, out kindex))
                {
                    if (kindex < 0 || kindex >= _count) return null;
                    RealWorldTerrainJsonItem item = _items[kindex];
                    return item[nextPart];
                }
            }

            if (key == "*") return this;
            if (int.TryParse(key, out kindex)) return this[kindex];
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

            for (int i = 0; i < _count; i++)
            {
                item = _items[i];
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
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Parse a string that contains an array
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Instance</returns>
        public static RealWorldTerrainJsonArray ParseArray(string json)
        {
            return RealWorldTerrainJson.Parse(json) as RealWorldTerrainJsonArray;
        }

        public override void ToJSON(StringBuilder b)
        {
            b.Append("[");
            for (int i = 0; i < _count; i++)
            {
                if (i != 0) b.Append(",");
                _items[i].ToJSON(b);
            }

            b.Append("]");
        }

        public override object Value(Type type)
        {
            if (RealWorldTerrainReflectionHelper.IsValueType(type)) return Activator.CreateInstance(type);
            return null;

        }
    }
}