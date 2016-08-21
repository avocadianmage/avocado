using FileDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace UtilityLib.Web.Scraping
{
    sealed class DownloadCache : IDownloadCache
    {
        struct Key
        {
            public readonly Uri Uri;
            public readonly WebHeaderCollection Headers;

            public Key(Uri uri, WebHeaderCollection headers)
            {
                Uri = uri;
                Headers = headers;
            }
        }

        readonly Dictionary<Key, string> cache = new Dictionary<Key, string>();
        
        public void Add(Uri uri, string path, WebHeaderCollection headers)
            => cache[new Key(uri, headers)] = path;

        public string Get(Uri uri, WebHeaderCollection headers)
        {
            var key = new Key(uri, headers);
            return cache.ContainsKey(key) ? cache[key] : null;
        }

        public void Invalidate(Uri uri)
        {
            cache.Where(p => p.Key.Uri == uri)
                .ToList()
                .ForEach(p => cache.Remove(p.Key));
        }
    }
}