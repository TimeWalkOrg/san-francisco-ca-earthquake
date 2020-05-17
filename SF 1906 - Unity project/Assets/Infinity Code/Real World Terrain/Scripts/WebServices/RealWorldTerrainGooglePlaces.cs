/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Text;
using InfinityCode.RealWorldTerrain.ExtraTypes;
using InfinityCode.RealWorldTerrain.Webservices.Base;
using InfinityCode.RealWorldTerrain.Webservices.Results;
using InfinityCode.RealWorldTerrain.XML;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Webservices
{
    /// <summary>
    /// The Google Places API allows you to query for place information on a variety of categories, such as: establishments, prominent points of interest, geographic locations, and more. \n
    /// You can search for places either by proximity or a text string. \n
    /// A Place Search returns a list of places along with summary information about each place.\n
    /// https://developers.google.com/places/web-service/search
    /// </summary>
    public class RealWorldTerrainGooglePlaces : RealWorldTerrainTextWebServiceBase
    {
        protected RealWorldTerrainGooglePlaces(string key, RequestParams p)
        {
            _status = RequestStatus.downloading;

            StringBuilder url = new StringBuilder();
            url.AppendFormat("https://maps.googleapis.com/maps/api/place/{0}/xml?sensor=false", p.typePath);
            if (!string.IsNullOrEmpty(key)) url.Append("&key=").Append(key);
            p.AppendParams(url);

            www = new RealWorldTerrainWWW(url.ToString());
            www.OnComplete += OnRequestComplete;
        }

        /// <summary>
        /// A Nearby Search lets you search for places within a specified area. \n
        /// You can refine your search request by supplying keywords or specifying the type of place you are searching for.
        /// </summary>
        /// <param name="lnglat">The longitude/latitude around which to retrieve place information. </param>
        /// <param name="radius">
        /// Defines the distance (in meters) within which to return place results. \n
        /// The maximum allowed radius is 50 000 meters.
        /// </param>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="keyword">A term to be matched against all content that Google has indexed for this place, including but not limited to name, type, and address, as well as customer reviews and other third-party content.</param>
        /// <param name="name">
        /// One or more terms to be matched against the names of places, separated with a space character. \n
        /// Results will be restricted to those containing the passed name values. \n
        /// Note that a place may have additional names associated with it, beyond its listed name. \n
        /// The API will try to match the passed name value against all of these names. \n
        /// As a result, places may be returned in the results whose listed names do not match the search term, but whose associated names do.
        /// </param>
        /// <param name="types">
        /// Restricts the results to places matching at least one of the specified types. \n
        /// Types should be separated with a pipe symbol (type1|type2|etc).\n
        /// See the list of supported types:\n
        /// https://developers.google.com/places/documentation/supported_types
        /// </param>
        /// <param name="minprice">
        /// Restricts results to only those places within the specified range. \n
        /// Valid values range between 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="maxprice">
        /// Restricts results to only those places within the specified range. \n
        /// Valid values range between 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="opennow">
        /// Returns only those places that are open for business at the time the query is sent. \n
        /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
        /// </param>
        /// <param name="rankBy">Specifies the order in which results are listed.</param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindNearby(Vector2 lnglat, int radius, string key, string keyword = null, string name = null, string types = null, int minprice = -1, int maxprice = -1, bool opennow = false, RankBy rankBy = RankBy.prominence)
        {
            NearbyParams p = new NearbyParams(lnglat, radius)
            {
                keyword = keyword,
                name = name,
                types = types,
            };

            if (minprice != -1) p.minprice = minprice;
            if (maxprice != -1) p.maxprice = maxprice;
            if (opennow) p.opennow = true;
            if (rankBy != RankBy.prominence) p.rankBy = rankBy;

            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// A Nearby Search lets you search for places within a specified area. \n
        /// You can refine your search request by supplying keywords or specifying the type of place you are searching for.
        /// </summary>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="p">The object containing the request parameters.</param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindNearby(string key, NearbyParams p)
        {
            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// Returns information about a set of places based on a string — for example "pizza in New York" or "shoe stores near Ottawa". \n
        /// The service responds with a list of places matching the text string and any location bias that has been set. \n
        /// The search response will include a list of places.
        /// </summary>
        /// <param name="query">
        /// The text string on which to search, for example: "restaurant". \n
        /// The Google Places service will return candidate matches based on this string and order the results based on their perceived relevance.
        /// </param>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="lnglat">The longitude/latitude around which to retrieve place information.</param>
        /// <param name="radius">
        /// Defines the distance (in meters) within which to bias place results. \n
        /// The maximum allowed radius is 50 000 meters. \n
        /// Results inside of this region will be ranked higher than results outside of the search circle; however, prominent results from outside of the search radius may be included.
        /// </param>
        /// <param name="language">The language code, indicating in which language the results should be returned, if possible. </param>
        /// <param name="types">
        /// Restricts the results to places matching at least one of the specified types. \n
        /// Types should be separated with a pipe symbol (type1|type2|etc). \n
        /// See the list of supported types:\n
        /// https://developers.google.com/maps/documentation/places/supported_types
        /// </param>
        /// <param name="minprice">
        /// Restricts results to only those places within the specified price level. \n
        /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="maxprice">
        /// Restricts results to only those places within the specified price level. \n
        /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="opennow">
        /// Returns only those places that are open for business at the time the query is sent. \n
        /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
        /// </param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindText(string query, string key, Vector2 lnglat = default(Vector2), int radius = -1, string language = null, string types = null, int minprice = -1, int maxprice = -1, bool opennow = false)
        {
            TextParams p = new TextParams(query)
            {
                language = language,
                types = types,
            };

            if (lnglat != default(Vector2)) p.lnglat = lnglat;
            if (radius != -1) p.radius = radius;
            if (minprice != -1) p.minprice = minprice;
            if (maxprice != -1) p.maxprice = maxprice;
            if (opennow) p.opennow = true;

            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// Returns information about a set of places based on a string — for example "pizza in New York" or "shoe stores near Ottawa". \n
        /// The service responds with a list of places matching the text string and any location bias that has been set. \n
        /// The search response will include a list of places.
        /// </summary>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="p">The object containing the request parameters.</param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindText(string key, TextParams p)
        {
            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// The Google Places API Radar Search Service allows you to search for up to 200 places at once, but with less detail than is typically returned from a Text Search or Nearby Search request. \n
        /// With Radar Search, you can create applications that help users identify specific areas of interest within a geographic area.
        /// </summary>
        /// <param name="lnglat">The longitude/latitude around which to retrieve place information.</param>
        /// <param name="radius">
        /// Defines the distance (in meters) within which to return place results. \n
        /// The maximum allowed radius is 50 000 meters.
        /// </param>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="keyword">A term to be matched against all content that Google has indexed for this place, including but not limited to name, type, and address, as well as customer reviews and other third-party content.</param>
        /// <param name="name">
        /// One or more terms to be matched against the names of places, separated by a space character. \n
        /// Results will be restricted to those containing the passed name values. \n
        /// Note that a place may have additional names associated with it, beyond its listed name. \n
        /// The API will try to match the passed name value against all of these names. \n
        /// As a result, places may be returned in the results whose listed names do not match the search term, but whose associated names do.
        /// </param>
        /// <param name="types">
        /// Restricts the results to places matching at least one of the specified types. \n
        /// Types should be separated with a pipe symbol (type1|type2|etc). \n
        /// See the list of supported types:\n
        /// https://developers.google.com/maps/documentation/places/supported_types
        /// </param>
        /// <param name="minprice">
        /// Restricts results to only those places within the specified price level. \n
        /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="maxprice">
        /// Restricts results to only those places within the specified price level. \n
        /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
        /// The exact amount indicated by a specific value will vary from region to region.
        /// </param>
        /// <param name="opennow">
        /// Returns only those places that are open for business at the time the query is sent. \n
        /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
        /// </param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindRadar(Vector2 lnglat, int radius, string key, string keyword = null, string name = null, string types = null, int minprice = -1, int maxprice = -1, bool opennow = false)
        {
            RadarParams p = new RadarParams(lnglat, radius)
            {
                keyword = keyword,
                name = name,
                types = types,
            };

            if (minprice != -1) p.minprice = minprice;
            if (maxprice != -1) p.maxprice = maxprice;
            if (opennow) p.opennow = true;

            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// The Google Places API Radar Search Service allows you to search for up to 200 places at once, but with less detail than is typically returned from a Text Search or Nearby Search request. \n
        /// With Radar Search, you can create applications that help users identify specific areas of interest within a geographic area.
        /// </summary>
        /// <param name="key">
        /// Your application's API key. \n
        /// This key identifies your application for purposes of quota management and so that places added from your application are made immediately available to your app. \n
        /// Visit the Google APIs Console to create an API Project and obtain your key.
        /// </param>
        /// <param name="p">The object containing the request parameters.</param>
        /// <returns>Query instance to the Google API.</returns>
        public static RealWorldTerrainGooglePlaces FindRadar(string key, RadarParams p)
        {
            return new RealWorldTerrainGooglePlaces(key, p);
        }

        /// <summary>
        /// Converts response into an array of results.
        /// </summary>
        /// <param name="response">Response of Google API.</param>
        /// <returns>Array of result.</returns>
        public static RealWorldTerrainPlacesResult[] GetResults(string response)
        {
            string nextPageToken;
            return GetResults(response, out nextPageToken);
        }

        /// <summary>
        /// Converts response into an array of results.
        /// </summary>
        /// <param name="response">Response of Google API.</param>
        /// <param name="nextPageToken">
        /// Contains a token that can be used to return up to 20 additional results.\n
        /// A next_page_token will not be returned if there are no additional results to display.\n
        /// The maximum number of results that can be returned is 60.\n
        /// There is a short delay between when a next_page_token is issued, and when it will become valid.
        /// </param>
        /// <returns>Array of result.</returns>
        public static RealWorldTerrainPlacesResult[] GetResults(string response, out string nextPageToken)
        {
            nextPageToken = null;

            try
            {
                RealWorldTerrainXML xml = RealWorldTerrainXML.Load(response);
                string status = xml.Find<string>("//status");
                if (status != "OK") return null;

                nextPageToken = xml.Find<string>("//next_page_token");
                RealWorldTerrainXMLList resNodes = xml.FindAll("//result");

                List<RealWorldTerrainPlacesResult> results = new List<RealWorldTerrainPlacesResult>(resNodes.count);
                foreach (RealWorldTerrainXML node in resNodes) results.Add(new RealWorldTerrainPlacesResult(node));
                return results.ToArray();
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message + "\n" + exception.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// The base class containing the request parameters.
        /// </summary>
        public abstract class RequestParams
        {
            public abstract string typePath { get; }
            public abstract void AppendParams(StringBuilder url);
        }

        /// <summary>
        /// Request parameters for Nearby Search
        /// </summary>
        public class NearbyParams : RequestParams
        {
            /// <summary>
            /// The longitude around which to retrieve place information.
            /// </summary>
            public double? longitude;

            /// <summary>
            /// The latitude around which to retrieve place information.
            /// </summary>
            public double? latitude;

            /// <summary>
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </summary>
            public int? radius;

            /// <summary>
            /// A term to be matched against all content that Google has indexed for this place, including but not limited to name, type, and address, as well as customer reviews and other third-party content.
            /// </summary>
            public string keyword;

            /// <summary>
            /// One or more terms to be matched against the names of places, separated with a space character. \n
            /// Results will be restricted to those containing the passed name values. \n
            /// Note that a place may have additional names associated with it, beyond its listed name. \n
            /// The API will try to match the passed name value against all of these names. \n
            /// As a result, places may be returned in the results whose listed names do not match the search term, but whose associated names do.
            /// </summary>
            public string name;

            /// <summary>
            /// Restricts the results to places matching at least one of the specified types. \n
            /// Types should be separated with a pipe symbol (type1|type2|etc).\n
            /// See the list of supported types:\n
            /// https://developers.google.com/places/documentation/supported_types
            /// </summary>
            public string types;

            /// <summary>
            /// Restricts results to only those places within the specified range. \n
            /// Valid values range between 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? minprice;

            /// <summary>
            /// Restricts results to only those places within the specified range. \n
            /// Valid values range between 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? maxprice;

            /// <summary>
            /// Returns only those places that are open for business at the time the query is sent. \n
            /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
            /// </summary>
            public bool? opennow;

            /// <summary>
            /// Specifies the order in which results are listed.
            /// </summary>
            public RankBy? rankBy;

            /// <summary>
            /// Returns the next 20 results from a previously run search. \n
            /// Setting a pagetoken parameter will execute a search with the same parameters used previously — all parameters other than pagetoken will be ignored. 
            /// </summary>
            public string pagetoken;

            /// <summary>
            /// Add this parameter (just the parameter name, with no associated value) to restrict your search to locations that are Zagat selected businesses.\n 
            /// This parameter must not include a true or false value. The zagatselected parameter is experimental, and is only available to Google Places API customers with a Premium Plan license.
            /// </summary>
            public bool? zagatselected;

            /// <summary>
            /// The longitude/latitude around which to retrieve place information.
            /// </summary>
            public Vector2 lnglat
            {
                get { return new Vector2((float)longitude.Value, (float)latitude.Value); }
                set
                {
                    longitude = value.x;
                    latitude = value.y;
                }
            }

            public override string typePath
            {
                get { return "nearbysearch"; }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="longitude">The longitude around which to retrieve place information.</param>
            /// <param name="latitude">The latitude around which to retrieve place information.</param>
            /// <param name="radius">
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </param>
            public NearbyParams(double longitude, double latitude, int radius)
            {
                this.longitude = longitude;
                this.latitude = latitude;
                this.radius = radius;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="lnglat">The longitude/latitude around which to retrieve place information.</param>
            /// <param name="radius">
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </param>
            public NearbyParams(Vector2 lnglat, int radius)
            {
                this.lnglat = lnglat;
                this.radius = radius;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="pagetoken">
            /// Returns the next 20 results from a previously run search. \n
            /// Setting a pagetoken parameter will execute a search with the same parameters used previously — all parameters other than pagetoken will be ignored. 
            /// </param>
            public NearbyParams(string pagetoken)
            {
                this.pagetoken = pagetoken;
            }

            public override void AppendParams(StringBuilder url)
            {
                if (latitude.HasValue && longitude.HasValue) url.Append("&location=").Append(latitude.Value).Append(",").Append(longitude.Value);
                if (radius.HasValue) url.Append("&radius=").Append(radius.Value);
                if (!string.IsNullOrEmpty(keyword)) url.Append("&keyword=").Append(keyword);
                if (!string.IsNullOrEmpty(name)) url.Append("&name=").Append(name);
                if (!string.IsNullOrEmpty(types)) url.Append("&types=").Append(types);
                if (minprice.HasValue) url.Append("&minprice=").Append(minprice.Value);
                if (maxprice.HasValue) url.Append("&maxprice=").Append(maxprice.Value);
                if (opennow.HasValue) url.Append("&opennow");
                if (rankBy.HasValue) url.Append("&rankby=").Append(rankBy.Value);
                if (!string.IsNullOrEmpty(pagetoken)) url.Append("&pagetoken=").Append(RealWorldTerrainWWW.EscapeURL(pagetoken));
                if (zagatselected.HasValue && zagatselected.Value) url.Append("&zagatselected");
            }
        }

        /// <summary>
        /// Request parameters for Text Search
        /// </summary>
        public class TextParams : RequestParams
        {
            /// <summary>
            /// The text string on which to search, for example: "restaurant". \n
            /// The Google Places service will return candidate matches based on this string and order the results based on their perceived relevance.
            /// </summary>
            public string query;

            /// <summary>
            /// The longitude around which to retrieve place information.
            /// </summary>
            public double? longitude;

            /// <summary>
            /// The latitude around which to retrieve place information.
            /// </summary>
            public double? latitude;

            /// <summary>
            /// Defines the distance (in meters) within which to bias place results. \n
            /// The maximum allowed radius is 50 000 meters. \n
            /// Results inside of this region will be ranked higher than results outside of the search circle; however, prominent results from outside of the search radius may be included.
            /// </summary>
            public int? radius;

            /// <summary>
            /// The language code, indicating in which language the results should be returned, if possible. 
            /// </summary>
            public string language;

            /// <summary>
            /// Restricts the results to places matching at least one of the specified types. \n
            /// Types should be separated with a pipe symbol (type1|type2|etc). \n
            /// See the list of supported types:\n
            /// https://developers.google.com/maps/documentation/places/supported_types
            /// </summary>
            public string types;

            /// <summary>
            /// Restricts results to only those places within the specified price level. \n
            /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? minprice;

            /// <summary>
            /// Restricts results to only those places within the specified price level. \n
            /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? maxprice;

            /// <summary>
            /// Returns only those places that are open for business at the time the query is sent. \n
            /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
            /// </summary>
            public bool? opennow;

            /// <summary>
            /// Returns the next 20 results from a previously run search. \n
            /// Setting a pagetoken parameter will execute a search with the same parameters used previously — all parameters other than pagetoken will be ignored. 
            /// </summary>
            public string pagetoken;

            /// <summary>
            /// Add this parameter (just the parameter name, with no associated value) to restrict your search to locations that are Zagat selected businesses.\n 
            /// This parameter must not include a true or false value. The zagatselected parameter is experimental, and is only available to Google Places API customers with a Premium Plan license.
            /// </summary>
            public bool? zagatselected;

            /// <summary>
            /// The longitude/latitude around which to retrieve place information.
            /// </summary>
            public Vector2 lnglat
            {
                get { return new Vector2((float)longitude.Value, (float)latitude.Value); }
                set
                {
                    longitude = value.x;
                    latitude = value.y;
                }
            }

            public override string typePath
            {
                get { return "textsearch"; }
            }

            /// <summary>
            /// Contstructor
            /// </summary>
            /// <param name="query">
            /// The text string on which to search, for example: "restaurant". \n
            /// The Google Places service will return candidate matches based on this string and order the results based on their perceived relevance.
            /// </param>
            public TextParams(string query)
            {
                this.query = query;
            }

            public override void AppendParams(StringBuilder url)
            {
                if (latitude.HasValue && longitude.HasValue) url.Append("&location=").Append(latitude.Value).Append(",").Append(longitude.Value);
                if (radius.HasValue) url.Append("&radius=").Append(radius.Value);
                if (!string.IsNullOrEmpty(types)) url.Append("&types=").Append(types);
                if (!string.IsNullOrEmpty(query)) url.Append("&query=").Append(RealWorldTerrainWWW.EscapeURL(query));
                if (!string.IsNullOrEmpty(language)) url.Append("&language=").Append(language);
                if (minprice.HasValue) url.Append("&minprice=").Append(minprice.Value);
                if (maxprice.HasValue) url.Append("&maxprice=").Append(maxprice.Value);
                if (opennow.HasValue && opennow.Value) url.Append("&opennow");
                if (!string.IsNullOrEmpty(pagetoken)) url.Append("&pagetoken=").Append(pagetoken);
                if (zagatselected.HasValue && zagatselected.Value) url.Append("&zagatselected");
            }
        }

        /// <summary>
        /// Request parameters for Radar Search
        /// </summary>
        public class RadarParams : RequestParams
        {
            /// <summary>
            /// The longitude around which to retrieve place information.
            /// </summary>
            public double? longitude;

            /// <summary>
            /// The latitude around which to retrieve place information.
            /// </summary>
            public double? latitude;

            /// <summary>
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </summary>
            public int? radius;

            /// <summary>
            /// A term to be matched against all content that Google has indexed for this place, including but not limited to name, type, and address, as well as customer reviews and other third-party content.
            /// </summary>
            public string keyword;

            /// <summary>
            /// One or more terms to be matched against the names of places, separated by a space character. \n
            /// Results will be restricted to those containing the passed name values. \n
            /// Note that a place may have additional names associated with it, beyond its listed name. \n
            /// The API will try to match the passed name value against all of these names. \n
            /// As a result, places may be returned in the results whose listed names do not match the search term, but whose associated names do.
            /// </summary>
            public string name;

            /// <summary>
            /// Restricts the results to places matching at least one of the specified types. \n
            /// Types should be separated with a pipe symbol (type1|type2|etc). \n
            /// See the list of supported types:\n
            /// https://developers.google.com/maps/documentation/places/supported_types
            /// </summary>
            public string types;

            /// <summary>
            /// Restricts results to only those places within the specified price level. \n
            /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? minprice;

            /// <summary>
            /// Restricts results to only those places within the specified price level. \n
            /// Valid values are in the range from 0 (most affordable) to 4 (most expensive), inclusive. \n
            /// The exact amount indicated by a specific value will vary from region to region.
            /// </summary>
            public int? maxprice;

            /// <summary>
            /// Returns only those places that are open for business at the time the query is sent. \n
            /// Places that do not specify opening hours in the Google Places database will not be returned if you include this parameter in your query.
            /// </summary>
            public bool? opennow;

            /// <summary>
            /// Add this parameter (just the parameter name, with no associated value) to restrict your search to locations that are Zagat selected businesses.\n 
            /// This parameter must not include a true or false value. The zagatselected parameter is experimental, and is only available to Google Places API customers with a Premium Plan license.
            /// </summary>
            public bool? zagatselected;

            /// <summary>
            /// The longitude/latitude around which to retrieve place information.
            /// </summary>
            public Vector2 lnglat
            {
                get { return new Vector2((float)longitude.Value, (float)latitude.Value); }
                set
                {
                    longitude = value.x;
                    latitude = value.y;
                }
            }

            public override string typePath
            {
                get { return "radarsearch"; }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="longitude">The longitude around which to retrieve place information.</param>
            /// <param name="latitude">The latitude around which to retrieve place information.</param>
            /// <param name="radius">
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </param>
            public RadarParams(double longitude, double latitude, int radius)
            {
                this.longitude = longitude;
                this.latitude = latitude;
                this.radius = radius;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="lnglat">The longitude/latitude around which to retrieve place information.</param>
            /// <param name="radius">
            /// Defines the distance (in meters) within which to return place results. \n
            /// The maximum allowed radius is 50 000 meters.
            /// </param>
            public RadarParams(Vector2 lnglat, int radius)
            {
                this.lnglat = lnglat;
                this.radius = radius;
            }

            public override void AppendParams(StringBuilder url)
            {
                if (latitude.HasValue && longitude.HasValue) url.Append("&location=").Append(latitude.Value).Append(",").Append(longitude.Value);
                if (radius.HasValue) url.Append("&radius=").Append(radius.Value);
                if (!string.IsNullOrEmpty(keyword)) url.Append("&keyword=").Append(keyword);
                if (!string.IsNullOrEmpty(name)) url.Append("&name=").Append(name);
                if (!string.IsNullOrEmpty(types)) url.Append("&types=").Append(types);
                if (minprice.HasValue) url.Append("&minprice=").Append(minprice.Value);
                if (maxprice.HasValue) url.Append("&maxprice=").Append(maxprice.Value);
                if (opennow.HasValue && opennow.Value) url.Append("&opennow");
                if (zagatselected.HasValue && zagatselected.Value) url.Append("&zagatselected");
            }
        }

        /// <summary>
        /// Specifies the order in which results are listed.
        /// </summary>
        public enum RankBy
        {
            /// <summary>
            /// This option sorts results based on their importance. \n
            /// Ranking will favor prominent places within the specified area. \n
            /// Prominence can be affected by a place's ranking in Google's index, global popularity, and other factors. 
            /// </summary>
            prominence,

            /// <summary>
            /// This option sorts results in ascending order by their distance from the specified location. \n
            /// When distance is specified, one or more of keyword, name, or types is required.
            /// </summary>
            distance
        }
    }
}