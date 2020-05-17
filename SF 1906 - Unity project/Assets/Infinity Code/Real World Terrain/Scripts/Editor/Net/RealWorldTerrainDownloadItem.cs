/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.IO;

namespace InfinityCode.RealWorldTerrain.Net
{
    public abstract class RealWorldTerrainDownloadItem
    {
        public delegate void OnCompleteHandler(ref byte[] data);
        public Action<RealWorldTerrainDownloadItem> OnSuccess;
        public Action<RealWorldTerrainDownloadItem> OnError;
        public Action<RealWorldTerrainDownloadItem> OnComplete;
        public event OnCompleteHandler OnData;

        public object exclusiveLock;
        public string url;
        public string filename;
        public long averageSize = 0;
        public bool generateErrorFile = false;
        public bool complete;
        public bool ignoreRequestProgress;

        private Dictionary<string, object> fields;

        private string _errorFilename;

        public object this[string key]
        {
            get
            {
                if (fields == null) return null;
                object obj = null;
                fields.TryGetValue(key, out obj);
                return obj;
            }
            set
            {
                if (fields == null) fields = new Dictionary<string, object>();
                fields[key] = value;
            }
        }

        private string errorFilename
        {
            get
            {
                if (string.IsNullOrEmpty(_errorFilename))
                {
                    if (string.IsNullOrEmpty(filename)) _errorFilename = String.Empty;
                    else _errorFilename = filename.Substring(0, filename.LastIndexOf(".") + 1) + "err";
                }
                return _errorFilename;
            }
        }

        public bool exists
        {
            get { return File.Exists(filename) || File.Exists(errorFilename); }
        }

        public abstract float progress { get; }


        public abstract void CheckComplete();

        public void CreateErrorFile()
        {
            if (!generateErrorFile) return;
            File.WriteAllBytes(errorFilename, new byte[] { });
        }

        public void DispatchCompete(ref byte[] data)
        {
            if (OnData != null) OnData(ref data);
            if (OnComplete != null) OnComplete(this);
        }

        public virtual void Dispose()
        {
            fields = null;

            OnSuccess = null;
            OnError = null;
            OnData = null;
        }

        public T GetField<T>(string key)
        {
            if (fields == null) return default(T);
            object obj = null;
            fields.TryGetValue(key, out obj);
            return (T) obj;
        }

        protected void SaveWWWData(byte[] bytes)
        {
            if (!string.IsNullOrEmpty(filename)) File.WriteAllBytes(filename, bytes);
        }

        public abstract void Start();
    }
}