/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityCode.RealWorldTerrain
{
    public static class RealWorldTerrainTextureProviderManager
    {
        private const string SATELLITE = "Satellite";
        private const string RELIEF = "Relief";
        private const string TERRAIN = "Terrain";
        private const string MAP = "Map";

        private static Provider[] providers;

        public static MapType FindMapType(string mapTypeID)
        {
            if (providers == null) InitProviders();

            if (string.IsNullOrEmpty(mapTypeID)) return providers[0].types[0];

            string[] parts = mapTypeID.Split('.');

            foreach (Provider provider in providers)
            {
                if (provider.id == parts[0])
                {
                    if (parts.Length == 1) return provider.types[0];
                    foreach (MapType type in provider.types)
                    {
                        if (type.id == parts[1]) return type;
                    }
                    return provider.types[0];
                }
            }
            return providers[0].types[0];
        }

        public static Provider[] GetProviders()
        {
            if (providers == null) InitProviders();
            return providers;
        }

        public static string[] GetProvidersTitle()
        {
            if (providers == null) InitProviders();
            return providers.Select(p => p.title).ToArray();
        }

        private static void InitProviders()
        {
            providers = new[]
            {
                new Provider("arcgis", "ArcGIS (Esri)")
                {
                    url = "https://server.arcgisonline.com/ArcGIS/rest/services/{variant}/MapServer/tile/{zoom}/{y}/{x}",
                    types = new []
                    {
                        new MapType("WorldImagery", "ag") { variant = "World_Imagery" },
                    }
                },
                new Provider("DigitalGlobe")
                {
                    url = "https://a.tiles.mapbox.com/v4/digitalglobe.{variant}/{zoom}/{x}/{y}.jpg?access_token={accesstoken}",
                    types = new []
                    {
                        new MapType("Satellite", "dg")
                        {
                            variant = "nal0g75k"
                        },
                    },
                    extraFields = new []
                    {
                        new ExtraField("Access Token", "accesstoken"),
                    },
                    help = new []
                    {
                        "1 map view = 15 tiles"
                    }
                },
                new Provider("Mapbox")
                {
                    types = new []
                    {
                        new MapType(SATELLITE, "mbs")
                        {
                            url = "https://a.tiles.mapbox.com/v4/mapbox.satellite/{zoom}/{x}/{y}.png?events=true&access_token={accesstoken}"
                        },
                        new MapType("Map", "mbm")
                        {
                            url = "https://api.mapbox.com/styles/v1/{userid}/{mapid}/tiles/256/{z}/{x}/{y}?access_token={accesstoken}",
                            extraFields = new []
                            {
                                new ExtraField("User ID", "userid"),
                                new ExtraField("Map ID", "mapid"),
                            }
                        },
                        new MapType("Classic", "mbc")
                        {
                            url = "https://a.tiles.mapbox.com/v4/{mapid}/{zoom}/{x}/{y}.png?&events=true&access_token={accesstoken}",
                            extraFields = new []
                            {
                                new ExtraField("Map ID", "mapid"),
                            },
                            help = new []
                            {
                                "Only raster tiles are supported."
                            }
                        }
                    },

                    extraFields = new []
                    {
                        new ExtraField("Access Token", "accesstoken"),
                    },

                    help = new []
                    {
                        "1 map view = 15 tiles"
                    }
                },
                new Provider("MapQuest")
                {
                    url = "https://a.tiles.mapbox.com/v4/{variant}/{zoom}/{x}/{y}.png?access_token={accesstoken}",
                    types = new []
                    {
                        new MapType(SATELLITE, "mq")
                        {
                            variant = "mapquest.satellite"
                        },
                    },
                    extraFields = new []
                    {
                        new ToggleExtraGroup("Anonymous", true, new []
                        {
                            new ExtraField("Access Token", "accesstoken", "pk.eyJ1IjoibWFwcXVlc3QiLCJhIjoiY2Q2N2RlMmNhY2NiZTRkMzlmZjJmZDk0NWU0ZGJlNTMifQ.mPRiEubbajc6a5y9ISgydg")
                        })
                    },
                },
                new Provider("mapy", "Mapy.CZ")
                {
                    url = "https://m[0-4].mapserver.mapy.cz/{variant}/{zoom}-{x}-{y}",
                    types = new []
                    {
                        new MapType(SATELLITE, "mcz")
                        {
                            variant = "ophoto-m"
                        },
                    }
                },
                new Provider("nokia", "Nokia Maps (here.com)")
                {
                    url = "https://[1-4].{prop2}.maps.cit.api.here.com/maptile/2.1/{prop}/newest/{variant}/{zoom}/{x}/{y}/256/png8?lg={lng}&app_id={appid}&app_code={appcode}",
                    prop = "maptile",
                    prop2 = "base",

                    types = new []
                    {
                        new MapType(SATELLITE, "n")
                        {
                            variant = "satellite.day",
                            prop2 = "aerial",
                        },
                    },

                    extraFields = new []
                    {
                        new ToggleExtraGroup("Anonymous", true, new []
                        {
                            new ExtraField("App ID", "appid", "xWVIueSv6JL0aJ5xqTxb"),
                            new ExtraField("App Code", "appcode", "djPZyynKsbTjIUDOBcHZ2g"),
                        })
                    }
                },
                new Provider("osm", "OpenStreetMap")
                {
                    types = new []
                    {
                        new MapType("Mapnik", "osmm")
                        {
                            url = "https://a.tile.openstreetmap.org/{zoom}/{x}/{y}.png"
                        },
                        new MapType("BlackAndWhite", "osmbw")
                        {
                            url = "http://a.tiles.wmflabs.org/bw-mapnik/{zoom}/{x}/{y}.png"
                        },
                        new MapType("DE", "osmde")
                        {
                            url = "http://a.tile.openstreetmap.de/tiles/osmde/{zoom}/{x}/{y}.png"
                        },
                        new MapType("France", "osmfr")
                        {
                            url = "https://a.tile.openstreetmap.fr/osmfr/{zoom}/{x}/{y}.png"
                        },
                        new MapType("HOT", "osmhot")
                        {
                            url = "https://a.tile.openstreetmap.fr/hot/{zoom}/{x}/{y}.png"
                        },
                    }
                },
                new Provider("virtualearth", "Virtual Earth (Bing Maps)")
                {
                    types = new []
                    {
                        new MapType("Aerial", "ve")
                        {
                            url = "https://t[0-4].ssl.ak.tiles.virtualearth.net/tiles/a{quad}.jpeg?mkt={lng}&g=1457&n=z",
                        },
                    }
                },
                new Provider("Custom")
                {
                    types = new []
                    {
                        new MapType("Custom", "custom") { isCustom = true }
                    }
                }
            };

            for (int i = 0; i < providers.Length; i++)
            {
                Provider provider = providers[i];
                provider.index = i;
                for (int j = 0; j < provider.types.Length; j++)
                {
                    MapType type = provider.types[j];
                    type.provider = provider;
                    type.fullID = provider.id + "." + type.id;
                    type.index = j;
                }
            }
        }

        public static string Upgrade(RealWorldTerrainTextureProvider providerID)
        {
            StringBuilder builder = new StringBuilder();
            if (providerID == RealWorldTerrainTextureProvider.arcGIS) builder.Append("arcgis");
            else if (providerID == RealWorldTerrainTextureProvider.google)
            {
                Debug.LogWarning("Support for Google Maps is removed, please use another provider.\nIf you really want to continue using Google Maps, you can do it using Provider - Custom.");
                return "arcgis";
            }
            else if (providerID == RealWorldTerrainTextureProvider.nokia) builder.Append("nokia");
            else if (providerID == RealWorldTerrainTextureProvider.mapQuest) builder.Append("mapquest");
            else if (providerID == RealWorldTerrainTextureProvider.virtualEarth) builder.Append("virtualearth");
            else if (providerID == RealWorldTerrainTextureProvider.openStreetMap) builder.Append("osm");
            else if (providerID == RealWorldTerrainTextureProvider.custom) builder.Append("custom").Append(".").Append("custom");
            else
            {
                Debug.LogWarning("Trying to upgrade provider failed. Please select the provider manually.");
                return "arcgis";
            }

            return builder.ToString();
        }

        public class Provider
        {
            /// <summary>
            /// ID of provider
            /// </summary>
            public readonly string id;

            /// <summary>
            /// Human-readable provider title.
            /// </summary>
            public readonly string title;

            /// <summary>
            /// Index of current provider.
            /// </summary>
            public int index;

            /// <summary>
            /// Extension. Token {ext}, that is being replaced in the URL.
            /// </summary>
            public string ext;

            /// <summary>
            /// Property. Token {prop}, that is being replaced in the URL.
            /// </summary>
            public string prop;

            /// <summary>
            /// Property. Token {prop2}, that is being replaced in the URL.
            /// </summary>
            public string prop2;

            public bool logUrl = false;
            public IExtraField[] extraFields;
            public string[] help;

            private string _url;
            private MapType[] _types;

            /// <summary>
            /// Array of map types available for the current provider.
            /// </summary>
            public MapType[] types
            {
                get { return _types; }
                set { _types = value; }
            }

            /// <summary>
            /// Gets / sets the URL pattern of tiles.
            /// </summary>
            public string url
            {
                get { return _url; }
                set
                {
                    _url = value;
                }
            }

            public Provider(string title) : this(title.ToLower(), title)
            {

            }

            public Provider(string id, string title)
            {
                this.id = id.ToLower();
                this.title = title;
            }

            /// <summary>
            /// Gets map type by index.
            /// </summary>
            /// <param name="index">Index of map type.</param>
            /// <param name="repeat">TRUE - Repeat index value, FALSE - Clamp index value.</param>
            /// <returns>Instance of map type.</returns>
            public MapType GetByIndex(int index, bool repeat = false)
            {
                if (repeat) index = Mathf.RoundToInt(Mathf.Repeat(index, _types.Length - 1));
                else index = Mathf.Clamp(index, 0, _types.Length);
                return _types[index];
            }
        }

       

        /// <summary>
        /// Class of map type
        /// </summary>
        public class MapType
        {
            /// <summary>
            /// ID of map type
            /// </summary>
            public readonly string id;

            public string filePrefix;

            public string fullID;

            /// <summary>
            /// Human-readable map type title.
            /// </summary>
            public readonly string title;

            /// <summary>
            /// Reference to provider instance.
            /// </summary>
            public Provider provider;

            /// <summary>
            /// Index of map type
            /// </summary>
            public int index;

            public IExtraField[] extraFields;

            /// <summary>
            /// Indicates that this is an custom provider.
            /// </summary>
            public bool isCustom;

            private string _ext;
            private string _url;
            private string _variant;
            private string _prop;
            private string _prop2;
            private bool? _logUrl;
            public string[] help;

            /// <summary>
            /// Extension. Token {ext}, that is being replaced in the URL.
            /// </summary>
            public string ext
            {
                get
                {
                    if (!string.IsNullOrEmpty(_ext)) return _ext;
                    if (!string.IsNullOrEmpty(provider.ext)) return provider.ext;
                    return string.Empty;
                }
                set { _ext = value; }
            }

            public bool logUrl
            {
                get
                {
                    if (_logUrl.HasValue) return _logUrl.Value;
                    return provider.logUrl;
                }
                set { _logUrl = value; }
            }

            /// <summary>
            /// Property. Token {prop}, that is being replaced in the URL.
            /// </summary>
            public string prop
            {
                get
                {
                    if (!string.IsNullOrEmpty(_prop)) return _prop;
                    return provider.prop;
                }
                set
                {
                    _prop = value;
                }
            }

            /// <summary>
            /// Property. Token {prop2}, that is being replaced in the URL.
            /// </summary>
            public string prop2
            {
                get { return string.IsNullOrEmpty(_prop2) ? provider.prop2 : _prop2; }
                set { _prop2 = value; }
            }

            /// <summary>
            /// Variant. Token {variant}, that is being replaced in the URL.
            /// </summary>
            public string variant
            {
                get { return _variant; }
                set
                {
                    _variant = value;
                }
            }

            /// <summary>
            /// Gets / sets the URL pattern of tiles.
            /// </summary>
            public string url
            {
                get
                {
                    if (!string.IsNullOrEmpty(_url)) return _url;
                    return provider.url;
                }
                set
                {
                    _url = value;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="title">Human-readable map type title.</param>
            public MapType(string title, string filePrefix) : this(title.ToLower(), title, filePrefix)
            {

            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="id">ID of map type.</param>
            /// <param name="title">Human-readable map type title.</param>
            public MapType(string id, string title, string filePrefix)
            {
                this.filePrefix = filePrefix;
                this.id = id;
                this.title = title;
            }

            public string GetSettings()
            {
                if (provider.extraFields == null) return null;

                StringBuilder builder = new StringBuilder();
                foreach (IExtraField field in provider.extraFields) field.SaveSettings(builder);
                return builder.ToString();
            }

            /// <summary>
            /// Gets the URL to download the tile texture.
            /// </summary>
            /// <param name="zoom">Tile zoom</param>
            /// <param name="x">Tile X</param>
            /// <param name="y">Tile Y</param>
            /// <returns>URL to tile texture.</returns>
            public string GetURL(int zoom, int x, int y)
            {
                return GetURL(zoom, x, y, url);
            }

            public string GetURL(int zoom, int x, int y, string url)
            {
                url = Regex.Replace(url, @"{\w+}", delegate (Match match)
                {
                    string v = match.Value.ToLower().Trim('{', '}');

                    if (v == "zoom") return zoom.ToString();
                    if (v == "z") return zoom.ToString();
                    if (v == "x") return x.ToString();
                    if (v == "y") return y.ToString();
                    if (v == "quad") return RealWorldTerrainUtils.TileToQuadKey(x, y, zoom);
                    if (v == "ext") return ext;
                    if (v == "prop") return prop;
                    if (v == "prop2") return prop2;
                    if (v == "variant") return variant;
                    if (TryUseExtraFields(ref v)) return v;
                    return v;
                });
                url = Regex.Replace(url, @"\[(\d+)-(\d+)\]", delegate (Match match)
                {
                    int v1 = int.Parse(match.Groups[1].Value);
                    int v2 = int.Parse(match.Groups[2].Value);
                    return Random.Range(v1, v2 + 1).ToString();
                });
                if (logUrl) Debug.Log(url);
                return url;
            }

            public void LoadSettings(string settings)
            {
                IExtraField[] fields = provider.extraFields;
                if (fields == null || string.IsNullOrEmpty(settings)) return;

                int i = 0;
                while (i < settings.Length)
                {
                    int titleLength = int.Parse(settings.Substring(i, 2));
                    i += 2;
                    string title = settings.Substring(i, titleLength);
                    i += titleLength;

                    int contentLengthSize = int.Parse(settings.Substring(i, 1));
                    i++;
                    int contentSize = int.Parse(settings.Substring(i, contentLengthSize));
                    i += contentLengthSize;

                    foreach (IExtraField field in fields) if (field.TryLoadSettings(title, settings, i, contentSize)) break;
                    i += contentSize;
                }
            }

            private bool TryUseExtraFields(ref string token)
            {
                if (provider.extraFields != null)
                {
                    foreach (IExtraField field in provider.extraFields)
                    {
                        string value;
                        if (field.GetTokenValue(token, false, out value))
                        {
                            token = value;
                            return true;
                        }
                    }
                }

                if (extraFields != null)
                {
                    foreach (IExtraField field in extraFields)
                    {
                        string value;
                        if (field.GetTokenValue(token, false, out value))
                        {
                            token = value;
                            return true;
                        }
                    }
                }

                return false;
            }

            public override string ToString()
            {
                return fullID;
            }
        }

        /// <summary>
        /// Group of toggle extra fields
        /// </summary>
        public class ToggleExtraGroup : IExtraField
        {
            /// <summary>
            /// Array of extra fields
            /// </summary>
            public IExtraField[] fields;

            /// <summary>
            /// Group title
            /// </summary>
            public string title;

            /// <summary>
            /// Group value
            /// </summary>
            public bool value = false;

            /// <summary>
            /// Group ID
            /// </summary>
            public string id;

            public ToggleExtraGroup(string title, bool value = false)
            {
                this.title = title;
                this.value = value;
            }

            public ToggleExtraGroup(string title, bool value, IExtraField[] fields) : this(title, value)
            {
                this.fields = fields;
            }

            public bool GetTokenValue(string token, bool useDefaultValue, out string value)
            {
                value = null;
                if (fields == null) return false;

                foreach (IExtraField field in fields)
                {
                    if (field.GetTokenValue(token, this.value || useDefaultValue, out value)) return true;
                }
                return false;
            }

            public void SaveSettings(StringBuilder builder)
            {
                int titleLength = title.Length;
                if (titleLength < 10) builder.Append("0");
                builder.Append(titleLength);
                builder.Append(title);

                StringBuilder dataBuilder = new StringBuilder();
                dataBuilder.Append(value ? 1 : 0);

                if (fields != null) foreach (IExtraField field in fields) field.SaveSettings(dataBuilder);

                builder.Append(dataBuilder.Length.ToString().Length);
                builder.Append(dataBuilder.Length);
                builder.Append(dataBuilder);
            }

            public bool TryLoadSettings(string title, string settings, int index, int contentSize)
            {
                if (this.title != title) return false;

                value = settings.Substring(index, 1) == "1";

                int i = index + 1;
                while (i < index + contentSize)
                {
                    int titleLength = int.Parse(settings.Substring(i, 2));
                    i += 2;
                    string fieldTitle = settings.Substring(i, titleLength);
                    i += titleLength;

                    int contentLengthSize = int.Parse(settings.Substring(i, 1));
                    i++;
                    int contentLength = int.Parse(settings.Substring(i, contentLengthSize));
                    i += contentLengthSize;

                    foreach (IExtraField field in fields) if (field.TryLoadSettings(fieldTitle, settings, i, contentLength)) break;

                    i += contentLength;
                }

                return true;
            }
        }

        /// <summary>
        /// Interface for extra fields tile provider
        /// </summary>
        public interface IExtraField
        {
            bool GetTokenValue(string token, bool useDefaultValue, out string value);
            void SaveSettings(StringBuilder builder);
            bool TryLoadSettings(string title, string settings, int index, int contentSize);
        }

        /// <summary>
        /// Class for extra field
        /// </summary>
        public class ExtraField : IExtraField
        {
            /// <summary>
            /// Title
            /// </summary>
            public string title;

            /// <summary>
            /// Value
            /// </summary>
            public string value;

            /// <summary>
            /// Default value
            /// </summary>
            public string defaultValue;

            /// <summary>
            /// Token (ID)
            /// </summary>
            public string token;

            public ExtraField(string title, string token)
            {
                this.title = title;
                this.token = token;
            }

            public ExtraField(string title, string token, string defaultValue) : this(title, token)
            {
                this.defaultValue = defaultValue;
            }

            public bool GetTokenValue(string token, bool useDefaultValue, out string value)
            {
                value = null;

                if (this.token == token)
                {
                    value = useDefaultValue ? defaultValue : this.value;
                    return true;
                }
                return false;
            }

            public void SaveSettings(StringBuilder builder)
            {
                int titleLength = title.Length;
                if (titleLength < 10) builder.Append("0");
                builder.Append(titleLength);
                builder.Append(title);

                if (string.IsNullOrEmpty(value)) builder.Append(1).Append(1).Append(0);
                else
                {
                    StringBuilder dataBuilder = new StringBuilder();
                    int valueLength = value.Length;
                    dataBuilder.Append(valueLength.ToString().Length);
                    dataBuilder.Append(valueLength);
                    dataBuilder.Append(value);
                    builder.Append(dataBuilder.Length.ToString().Length);
                    builder.Append(dataBuilder.Length);
                    builder.Append(dataBuilder);
                }
            }

            public bool TryLoadSettings(string title, string settings, int index, int contentSize)
            {
                if (this.title != title) return false;

                int lengthSize = int.Parse(settings.Substring(index, 1));
                if (lengthSize == 0) value = "";
                else
                {
                    index++;
                    int length = int.Parse(settings.Substring(index, lengthSize));
                    index += lengthSize;
                    value = settings.Substring(index, length);
                }

                return true;
            }
        }
    }
}
