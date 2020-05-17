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
    /// This class is used to search for a location by address.\n
    /// https://developers.google.com/maps/documentation/geocoding/intro
    /// </summary>
    public class RealWorldTerrainGoogleGeocoding : RealWorldTerrainTextWebServiceBase
    {
        private RealWorldTerrainGoogleGeocoding(RequestParams p)
        {
            _status = RequestStatus.downloading;

            StringBuilder url = new StringBuilder("https://maps.googleapis.com/maps/api/geocode/xml?sensor=false");
            p.GenerateURL(url);
            www = new RealWorldTerrainWWW(url.ToString());
            www.OnComplete += OnRequestComplete;
        }

        /// <summary>
        /// Creates a new request for a location search using object with parameters.
        /// </summary>
        /// <param name="p">Parameters of request</param>
        /// <returns>Instance of the search query.</returns>
        public static RealWorldTerrainGoogleGeocoding Find(RequestParams p)
        {
            return new RealWorldTerrainGoogleGeocoding(p);
        }

        /// <summary>
        /// Converts response into an array of results.
        /// </summary>
        /// <param name="response">Response of Google API.</param>
        /// <returns>Array of result.</returns>
        public static RealWorldTerrainGoogleGeocodingResult[] GetResults(string response)
        {
            try
            {
                RealWorldTerrainXML xml = RealWorldTerrainXML.Load(response);
                string status = xml.Find<string>("//status");
                if (status != "OK") return null;

                List<RealWorldTerrainGoogleGeocodingResult> results = new List<RealWorldTerrainGoogleGeocodingResult>();

                RealWorldTerrainXMLList resNodes = xml.FindAll("//result");

                foreach (RealWorldTerrainXML node in resNodes)
                {
                    results.Add(new RealWorldTerrainGoogleGeocodingResult(node));
                }

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
            /// <summary>
            /// Your application's API key. This key identifies your application for purposes of quota management.
            /// </summary>
            public string key;

            /// <summary>
            /// The language in which to return results. 
            /// List of supported languages:
            /// https://developers.google.com/maps/faq#languagesupport
            /// </summary>
            public string language;

            /// <summary>
            /// Available to Google Maps APIs Premium Plan customers but not to holders of a previous Maps API for Business license.
            /// </summary>
            public string client;

            /// <summary>
            /// Uses instead of an API key to authenticate requests.
            /// </summary>
            public string signature;

            internal virtual void GenerateURL(StringBuilder url)
            {
                if (!string.IsNullOrEmpty(key)) url.Append("&key=").Append(key);
                if (!string.IsNullOrEmpty(language)) url.Append("&language=").Append(language);
                if (!string.IsNullOrEmpty(client)) url.Append("&client=").Append(client);
                if (!string.IsNullOrEmpty(signature)) url.Append("&signature=").Append(signature);
            }
        }

        /// <summary>
        /// Request parameters for Geocoding
        /// </summary>
        public class GeocodingParams : RequestParams
        {
            /// <summary>
            /// The street address that you want to geocode, in the format used by the national postal service of the country concerned. \n
            /// Additional address elements such as business names and unit, suite or floor numbers should be avoided.
            /// </summary>
            public string address;

            /// <summary>
            /// A component filter for which you wish to obtain a geocode. \n
            /// See Component Filtering for more information. \n
            /// https://developers.google.com/maps/documentation/geocoding/intro?hl=en#ComponentFiltering \n
            /// The components filter will also be accepted as an optional parameter if an address is provided. 
            /// </summary>
            public string components;

            /// <summary>
            /// The bounding box of the viewport within which to bias geocode results more prominently. \n
            /// This parameter will only influence, not fully restrict, results from the geocoder.
            /// </summary>
            public RealWorldTerrainGeoRect bounds;

            /// <summary>
            /// The region code, specified as a ccTLD ("top-level domain") two-character value. \n
            /// This parameter will only influence, not fully restrict, results from the geocoder.
            /// </summary>
            public string region;

            /// <summary>
            /// Constructor
            /// </summary>
            public GeocodingParams()
            {

            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="address">
            /// The street address that you want to geocode, in the format used by the national postal service of the country concerned. \n
            /// Additional address elements such as business names and unit, suite or floor numbers should be avoided.
            /// </param>
            public GeocodingParams(string address)
            {
                this.address = address;
            }

            internal override void GenerateURL(StringBuilder url)
            {
                base.GenerateURL(url);

                if (!string.IsNullOrEmpty(address)) url.Append("&address=").Append(RealWorldTerrainWWW.EscapeURL(address));
                if (!string.IsNullOrEmpty(components)) url.Append("&components=").Append(components);
                if (bounds != null) url.Append("&bounds=").Append(bounds.bottom).Append(",").Append(bounds.left).Append("|").Append(bounds.top).Append(",").Append(bounds.right);
                if (!string.IsNullOrEmpty(region)) url.Append("&region=").Append(region);
            }
        }

        /// <summary>
        /// Request parameters for Reverse Geocoding
        /// </summary>
        public class ReverseGeocodingParams : RequestParams
        {
            /// <summary>
            /// The longitude value specifying the location for which you wish to obtain the closest, human-readable address. 
            /// </summary>
            public double? longitude;

            /// <summary>
            /// The latitude value specifying the location for which you wish to obtain the closest, human-readable address. 
            /// </summary>
            public double? latitude;

            /// <summary>
            /// The place ID of the place for which you wish to obtain the human-readable address. \n
            /// The place ID is a unique identifier that can be used with other Google APIs. \n
            /// For example, you can use the placeID returned by the Google Maps Roads API to get the address for a snapped point. \n
            /// For more information about place IDs, see the place ID overview. \n
            /// The place ID may only be specified if the request includes an API key or a Google Maps APIs Premium Plan client ID. 
            /// </summary>
            public string placeId;

            /// <summary>
            /// One or more address types, separated by a pipe (|). \n
            /// Examples of address types: country, street_address, postal_code. \n
            /// For a full list of allowable values, see the address types on this page:\n
            /// https://developers.google.com/maps/documentation/geocoding/intro?hl=en#Types \n
            /// Specifying a type will restrict the results to this type. \n
            /// If multiple types are specified, the API will return all addresses that match any of the types. \n
            /// Note: This parameter is available only for requests that include an API key or a client ID.
            /// </summary>
            public string result_type;

            /// <summary>
            /// One or more location types, separated by a pipe (|). \n
            /// https://developers.google.com/maps/documentation/geocoding/intro?hl=en#ReverseGeocoding \n
            /// Specifying a type will restrict the results to this type. \n
            /// If multiple types are specified, the API will return all addresses that match any of the types. \n
            /// Note: This parameter is available only for requests that include an API key or a client ID.
            /// </summary>
            public string location_type;

            /// <summary>
            /// The longitude and latitude values specifying the location for which you wish to obtain the closest, human-readable address. 
            /// </summary>
            public Vector2? location
            {
                get
                {
                    return new Vector2(longitude.HasValue ? (float)longitude.Value : 0, latitude.HasValue ? (float)latitude.Value : 0);
                }
                set
                {
                    if (value.HasValue)
                    {
                        longitude = value.Value.x;
                        latitude = value.Value.y;
                    }
                    else
                    {
                        longitude = null;
                        latitude = null;
                    }
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="longitude">The longitude value specifying the location for which you wish to obtain the closest, human-readable address. </param>
            /// <param name="latitude">The latitude value specifying the location for which you wish to obtain the closest, human-readable address. </param>
            public ReverseGeocodingParams(double longitude, double latitude)
            {
                this.longitude = longitude;
                this.latitude = latitude;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="location">The longitude and latitude values specifying the location for which you wish to obtain the closest, human-readable address. </param>
            public ReverseGeocodingParams(Vector2 location)
            {
                this.location = location;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="placeId">The place ID of the place for which you wish to obtain the human-readable address. \n
            /// The place ID is a unique identifier that can be used with other Google APIs. \n
            /// For example, you can use the placeID returned by the Google Maps Roads API to get the address for a snapped point. \n
            /// For more information about place IDs, see the place ID overview. \n
            /// The place ID may only be specified if the request includes an API key or a Google Maps APIs Premium Plan client ID. 
            /// </param>
            public ReverseGeocodingParams(string placeId)
            {
                this.placeId = placeId;
            }

            internal override void GenerateURL(StringBuilder url)
            {
                base.GenerateURL(url);

                if (longitude.HasValue && latitude.HasValue) url.Append("&latlng=").Append(latitude.Value.ToString(RealWorldTerrainCultureInfo.numberFormat)).Append(",").Append(longitude.Value.ToString(RealWorldTerrainCultureInfo.numberFormat));
                else if (!string.IsNullOrEmpty(placeId)) url.Append("&placeId=").Append(placeId);
                else throw new Exception("You must specify latitude and longitude, location, or placeId.");

                if (!string.IsNullOrEmpty(result_type)) url.Append("&result_type=").Append(result_type);
                if (!string.IsNullOrEmpty(location_type)) url.Append("&location_type=").Append(location_type);
            }
        }
    }
}