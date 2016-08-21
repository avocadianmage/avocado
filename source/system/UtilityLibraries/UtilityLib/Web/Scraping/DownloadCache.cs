using FileDownloader;
using System;
using System.Collections.Generic;
using System.Net;

namespace UtilityLib.Web.Scraping
{
    sealed class DownloadCache : IDownloadCache
    {
        readonly Dictionary<Uri, string> cache = new Dictionary<Uri, string>();
        
        public void Add(Uri uri, string path, WebHeaderCollection headers)
            => cache[uri] = path;

        public string Get(Uri uri, WebHeaderCollection headers)
            => cache.ContainsKey(uri) ? cache[uri] : null;

        public void Invalidate(Uri uri)
        {
            if (cache.ContainsKey(uri)) cache.Remove(uri);
        }
    }
}