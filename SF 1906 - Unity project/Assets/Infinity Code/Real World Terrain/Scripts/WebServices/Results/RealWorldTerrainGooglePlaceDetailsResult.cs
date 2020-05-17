/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using InfinityCode.RealWorldTerrain.XML;
using UnityEngine;

namespace InfinityCode.RealWorldTerrain.Webservices.Results
{
    /// <summary>
    /// Result of Google Maps Place Details query.
    /// </summary>
    public class RealWorldTerrainGooglePlaceDetailsResult
    {
        /// <summary>
        /// Human-readable address of this place.\n
        /// Often this address is equivalent to the "postal address," which sometimes differs from country to country.
        /// </summary>
        public string formatted_address;

        /// <summary>
        /// Phone number in its local format.\n
        /// For example, the formatted_phone_number for Google's Sydney, Australia office is (02) 9374 4000.
        /// </summary>
        public string formatted_phone_number;

        /// <summary>
        /// Geographic coordinates of place.
        /// </summary>
        public Vector2 location;

        /// <summary>
        /// URL of a suggested icon which may be displayed to the user when indicating this result on a map.
        /// </summary>
        public string icon;

        /// <summary>
        /// unique stable identifier denoting this place.\n
        /// This identifier may not be used to retrieve information about this place, but can be used to consolidate data about this place, and to verify the identity of a place across separate searches.\n
        /// As IDs can occasionally change, it's recommended that the stored ID for a place be compared with the ID returned in later Details requests for the same place, and updated if necessary.\n
        /// Note: The id is now deprecated in favor of place_id.
        /// </summary>
        public string id;

        /// <summary>
        /// Phone number in international format.\n
        /// International format includes the country code, and is prefixed with the plus (+) sign.\n
        /// For example, the international_phone_number for Google's Sydney, Australia office is +61 2 9374 4000. 
        /// </summary>
        public string international_phone_number;

        /// <summary>
        /// Human-readable name for the returned result.\n
        /// For establishment results, this is usually the canonicalized business name.
        /// </summary>
        public string name;

        /// <summary>
        /// Reference to XML node.
        /// </summary>
        public RealWorldTerrainXML node;

        /// <summary>
        /// Array of photo objects, each containing a reference to an image.\n
        /// A Place Details request may return up to ten photos.
        /// </summary>
        public RealWorldTerrainPlacesResult.Photo[] photos;

        /// <summary>
        /// A textual identifier that uniquely identifies a place.
        /// </summary>
        public string place_id;

        /// <summary>
        ///  The price level of the place, on a scale of 0 to 4. 
        /// The exact amount indicated by a specific value will vary from region to region. 
        /// Price levels are interpreted as follows:
        /// -1 - Unknown
        /// 0 — Free
        /// 1 — Inexpensive
        /// 2 — Moderate
        /// 3 — Expensive
        /// 4 — Very Expensive
        /// </summary>
        public int price_level = -1;

        /// <summary>
        /// Place's rating, from 1.0 to 5.0, based on aggregated user reviews.
        /// </summary>
        public float rating;

        /// <summary>
        /// Unique token that you can use to retrieve additional information about this place in a Place Details request. \n
        /// Although this token uniquely identifies the place, the converse is not true. \n
        /// A place may have many valid reference tokens. \n
        /// It's not guaranteed that the same token will be returned for any given place across different searches. \n
        /// Note: The reference is now deprecated in favor of place_id.
        /// </summary>
        public string reference;

        /// <summary>
        /// Array of feature types describing the given result. \n
        /// XML responses include multiple type elements if more than one type is assigned to the result.
        /// </summary>
        public string[] types;

        /// <summary>
        /// URL of the official Google page for this place.\n
        /// This will be the establishment's Google+ page if the Google+ page exists, otherwise it will be the Google-owned page that contains the best available information about the place.\n
        /// Applications must link to or embed this page on any screen that shows detailed results about the place to the user.
        /// </summary>
        public string url;

        /// <summary>
        /// Number of minutes this place’s current timezone is offset from UTC.\n
        /// For example, for places in Sydney, Australia during daylight saving time this would be 660 (+11 hours from UTC), and for places in California outside of daylight saving time this would be -480 (-8 hours from UTC).
        /// </summary>
        public string utc_offset;

        /// <summary>
        /// Lists a simplified address for the place, including the street name, street number, and locality, but not the province/state, postal code, or country.\n
        /// For example, Google's Sydney, Australia office has a vicinity value of 48 Pirrama Road, Pyrmont.
        /// </summary>
        public string vicinity;

        /// <summary>
        /// Lists the authoritative website for this place, such as a business' homepage.
        /// </summary>
        public string website;

        public RealWorldTerrainGooglePlaceDetailsResult()
        {

        }

        /// <summary>
        /// Constructor of RealWorldTerrainGooglePlaceDetailsResult.
        /// </summary>
        /// <param name="node">Place node from response.</param>
        public RealWorldTerrainGooglePlaceDetailsResult(RealWorldTerrainXML node)
        {
            this.node = node;
            formatted_address = node.Get("formatted_address");
            formatted_phone_number = node.Get("formatted_phone_number");

            RealWorldTerrainXML locationNode = node.Find("geometry/location");
            if (!locationNode.isNull) location = new Vector2(locationNode.Get<float>("lng"), locationNode.Get<float>("lat"));

            icon = node.Get("icon");
            id = node.Get("id");
            international_phone_number = node.Get("international_phone_number");
            name = node.Get("name");

            RealWorldTerrainXMLList photosList = node.FindAll("photo");
            photos = new RealWorldTerrainPlacesResult.Photo[photosList.count];
            for (int i = 0; i < photosList.count; i++) photos[i] = new RealWorldTerrainPlacesResult.Photo(photosList[i]);

            place_id = node.Get<string>("place_id");
            price_level = node.Get("price_level", -1);
            rating = node.Get<float>("rating");
            reference = node.Get("reference");

            RealWorldTerrainXMLList typeNode = node.FindAll("type");
            types = new string[typeNode.count];
            for (int i = 0; i < typeNode.count; i++) types[i] = typeNode[i].Value();

            url = node.Get("url");
            utc_offset = node.Get("utc_offset");
            vicinity = node.Get("vicinity");
            website = node.Get("website");
        }
    }
}