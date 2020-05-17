/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.ExtraTypes;

namespace InfinityCode.RealWorldTerrain.Webservices
{
    /// <summary>
    /// The base class for working with the web services.
    /// </summary>
    public abstract class RealWorldTerrainWebServiceBase
    {
        /// <summary>
        /// Event that occurs when the current request instance is disposed.
        /// </summary>
        public Action<RealWorldTerrainWebServiceBase> OnDispose;

        /// <summary>
        /// Event that occurs after OnComplete, when the response from webservice processed.
        /// </summary>
        public Action<RealWorldTerrainWebServiceBase> OnFinish;

        /// <summary>
        /// In this variable you can put any data that you need to work with requests.
        /// </summary>
        public object customData;

        protected RequestStatus _status;
        protected RealWorldTerrainWWW www;

        /// <summary>
        /// Gets the current status of the request to webservice.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public RequestStatus status
        {
            get { return _status; }
        }

        /// <summary>
        /// Destroys the current request to webservice.
        /// </summary>
        public abstract void Destroy();

        public enum RequestStatus
        {
            downloading,
            success,
            error,
            disposed
        }
    }
}