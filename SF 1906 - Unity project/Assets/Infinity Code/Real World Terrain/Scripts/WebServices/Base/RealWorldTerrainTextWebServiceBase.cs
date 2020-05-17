/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using InfinityCode.RealWorldTerrain.ExtraTypes;

namespace InfinityCode.RealWorldTerrain.Webservices.Base
{
    /// <summary>
    /// The base class for working with the web services returns text response.
    /// </summary>
    public abstract class RealWorldTerrainTextWebServiceBase : RealWorldTerrainWebServiceBase
    {
        /// <summary>
        /// Event that occurs when a response is received from webservice.
        /// </summary>
        public Action<string> OnComplete;

        protected string _response;

        /// <summary>
        /// Gets a response from webservice.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string response
        {
            get { return _response; }
        }

        public override void Destroy()
        {
            if (OnDispose != null) OnDispose(this);

            www = null;
            _response = string.Empty;
            _status = RequestStatus.disposed;
            customData = null;
            OnComplete = null;
            OnFinish = null;
        }

        /// <summary>
        /// Checks whether the response from webservice.
        /// </summary>
        protected void OnRequestComplete(RealWorldTerrainWWW www)
        {
            if (www != null && www.isDone)
            {
                _status = string.IsNullOrEmpty(www.error) ? RequestStatus.success : RequestStatus.error;
                _response = _status == RequestStatus.success ? www.text : www.error;

                if (OnComplete != null) OnComplete(_response);
                if (OnFinish != null) OnFinish(this);

                _status = RequestStatus.disposed;
                _response = null;
                this.www = null;
                customData = null;
            }
        }
    }
}