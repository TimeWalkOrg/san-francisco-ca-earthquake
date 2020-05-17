/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InfinityCode.RealWorldTerrain.ExtraTypes
{
    /// <summary>
    /// The wrapper class for WWW.\n
    /// It allows you to control requests.\n
    /// </summary>
    public class RealWorldTerrainWWW
    {
        /// <summary>
        /// Event that occurs when a request is completed.
        /// </summary>
        public Action<RealWorldTerrainWWW> OnComplete;

        /// <summary>
        /// In this variable you can put any data that you need to work with request.
        /// </summary>
        public object customData;

#if UNITY_2018_3_OR_NEWER
        private UnityWebRequest www;
#else
        private WWW www;
#endif
        private RequestType type;
        private byte[] _bytes;
        private string _error;
        private bool _isDone;
        private string _url;
        private string responseHeadersString;
        private string _id;
        private IEnumerator waitResponse;
        private RealWorldTerrainWWWHelper _helper;


        /// <summary>
        /// Returns the contents of the fetched web page as a byte array.
        /// </summary>
        public byte[] bytes
        {
            get
            {
                if (type == RequestType.www)
                {
#if UNITY_2018_3_OR_NEWER
                    return www.downloadHandler.data;
#else
                    return www.bytes;
#endif
                }
                return _bytes;
            }
        }

        /// <summary>
        /// The number of bytes downloaded by this query.
        /// </summary>
        public int bytesDownloaded
        {
            get
            {
                if (type == RequestType.www)
                {
#if UNITY_2018_3_OR_NEWER
                    return (int)www.downloadedBytes;
#else
                    return www.bytesDownloaded;
#endif
                }
                return _bytes != null ? _bytes.Length : 0;
            }
        }

        /// <summary>
        /// Returns an error message if there was an error during the download.
        /// </summary>
        public string error
        {
            get
            {
                if (type == RequestType.www) return www.error;
                return _error;
            }
        }

        private RealWorldTerrainWWWHelper helper
        {
            get
            {
                if (_helper != null) return _helper;
                GameObject go = new GameObject("WWW Helper");
                return _helper = go.AddComponent<RealWorldTerrainWWWHelper>();
            }
        }

        /// <summary>
        /// ID of query.
        /// </summary>
        public string id
        {
            get { return _id; }
        }

        /// <summary>
        /// Is the download already finished?
        /// </summary>
        public bool isDone
        {
            get
            {
                if (type == RequestType.www) return www.isDone;
                return _isDone;
            }
        }

        private bool isPlaying
        {
            get
            {
#if UNITY_EDITOR
                return EditorApplication.isPlaying;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Dictionary of headers returned by the request.
        /// </summary>
        public Dictionary<string, string> responseHeaders
        {
            get
            {
                if (!isDone) throw new UnityException("WWW is not finished downloading yet");

                if (type == RequestType.www)
                {
#if UNITY_2018_3_OR_NEWER
                    return www.GetResponseHeaders();
#else
                    return www.responseHeaders;
#endif
                }
                return ParseHTTPHeaderString(responseHeadersString);
            }
        }

        /// <summary>
        /// Returns the contents of the fetched web page as a string.
        /// </summary>
        public string text
        {
            get
            {
                if (type == RequestType.www)
                {
#if UNITY_2018_3_OR_NEWER
                    return www.downloadHandler.text;
#else
                    return www.text;
#endif
                }
                return _bytes != null ? GetTextEncoder().GetString(_bytes, 0, _bytes.Length) : null;
            }
        }

        /// <summary>
        /// The URL of this request.
        /// </summary>
        public string url
        {
            get { return _url; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL of request.</param>
        public RealWorldTerrainWWW(string url)
        {
            _url = url;
            type = RequestType.www;

#if UNITY_2018_3_OR_NEWER
            www = UnityWebRequest.Get(url);
            www.SendWebRequest();
#else
            www = new WWW(url);
#endif

            if (isPlaying) helper.StartCoroutine(WaitResponse());
#if UNITY_EDITOR
            else EditorApplication.update += CheckWWWComplete;
#endif
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">URL of request.</param>
        /// <param name="type">Type of request.</param>
        /// <param name="reqID">Request ID</param>
        public RealWorldTerrainWWW(string url, RequestType type, string reqID)
        {
            this.type = type;
            _url = url;
            _id = reqID;
            if (type == RequestType.www)
            {
#if UNITY_2018_3_OR_NEWER
                www = UnityWebRequest.Get(url);
                www.SendWebRequest();
#else
                www = new WWW(url);
#endif

                if (isPlaying) helper.StartCoroutine(WaitResponse());
#if UNITY_EDITOR
                else EditorApplication.update += CheckWWWComplete;
#endif

            }
        }

        private void CheckWWWComplete()
        {
            if (www.isDone)
            {
                Finish();
#if UNITY_EDITOR
                EditorApplication.update -= CheckWWWComplete;
#endif
            }
        }

        /// <summary>
        /// Disposes of an existing object.
        /// </summary>
        public void Dispose()
        {
            if (www != null && !www.isDone) www.Dispose();
            www = null;
            customData = null;
            if (waitResponse != null) helper.StopCoroutine(waitResponse);

            if (isPlaying) Object.Destroy(helper.gameObject);
            else Object.DestroyImmediate(helper.gameObject);
        }

        /// <summary>
        /// Escapes characters in a string to ensure they are URL-friendly.
        /// </summary>
        /// <param name="s">A string with characters to be escaped.</param>
        /// <returns>Escaped string.</returns>
        public static string EscapeURL(string s)
        {
#if UNITY_2018_3_OR_NEWER
            return UnityWebRequest.EscapeURL(s);
#else
            return WWW.EscapeURL(s);
#endif
        }

        private void Finish()
        {
            if (OnComplete != null) OnComplete(this);
            Dispose();
        }

        private Encoding GetTextEncoder()
        {
            string str;
            if (responseHeaders.TryGetValue("CONTENT-TYPE", out str))
            {
                int index = str.IndexOf("charset", StringComparison.OrdinalIgnoreCase);
                if (index > -1)
                {
                    int num2 = str.IndexOf('=', index);
                    if (num2 > -1)
                    {
                        char[] trimChars = { '\'', '"' };
                        string name = str.Substring(num2 + 1).Trim().Trim(trimChars).Trim();
                        int length = name.IndexOf(';');
                        if (length > -1)
                        {
                            name = name.Substring(0, length);
                        }
                        try
                        {
                            return Encoding.GetEncoding(name);
                        }
                        catch (Exception)
                        {
                            Debug.Log("Unsupported encoding: '" + name + "'");
                        }
                    }
                }
            }
            return Encoding.UTF8;
        }

        /// <summary>
        /// Replaces the contents of an existing Texture2D with an image from the downloaded data.
        /// </summary>
        /// <param name="tex">An existing texture object to be overwritten with the image data.</param>
        public void LoadImageIntoTexture(Texture2D tex)
        {
            if (tex == null) throw new Exception("Texture is null");

            if (type == RequestType.www)
            {
#if UNITY_2018_3_OR_NEWER
                tex.LoadImage(www.downloadHandler.data);
#else
                www.LoadImageIntoTexture(tex);
#endif
            }
            else tex.LoadImage(_bytes);
        }

        internal static Dictionary<string, string> ParseHTTPHeaderString(string input)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(input)) return dictionary;

            StringReader reader = new StringReader(input);
            int num = 0;
            while (true)
            {
                string str = reader.ReadLine();
                if (str == null)
                {
                    return dictionary;
                }
                if ((num++ == 0) && str.StartsWith("HTTP"))
                {
                    dictionary["STATUS"] = str;
                }
                else
                {
                    int index = str.IndexOf(": ");
                    if (index != -1)
                    {
                        string str2 = str.Substring(0, index).ToUpper();
                        string str3 = str.Substring(index + 2);
                        dictionary[str2] = str3;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the contents and headers of the response for type = direct.
        /// </summary>
        /// <param name="responseHeadersString">Headers of response.</param>
        /// <param name="_bytes">Content of response.</param>
        public void SetBytes(string responseHeadersString, byte[] _bytes)
        {
            if (type == RequestType.www) throw new Exception("RealWorldTerrainWWW.SetBytes available only for type = direct.");

            this.responseHeadersString = responseHeadersString;
            this._bytes = _bytes;
            _isDone = true;
            Finish();
        }

        /// <summary>
        /// Sets the error for type = direct.
        /// </summary>
        /// <param name="errorStr">Error message.</param>
        public void SetError(string errorStr)
        {
            if (type == RequestType.www) throw new Exception("RealWorldTerrainWWW.SetError available only for type = direct.");
            _error = errorStr;
            _isDone = true;
            Finish();

        }

        private IEnumerator WaitResponse()
        {
            yield return www;
            waitResponse = null;
            Finish();
        }

        /// <summary>
        /// Type of request.
        /// </summary>
        public enum RequestType
        {
            /// <summary>
            /// The request will be processed independently.
            /// </summary>
            direct,

            /// <summary>
            /// The request will be processed using WWW class.
            /// </summary>
            www
        }
    }
}