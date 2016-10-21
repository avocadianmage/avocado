using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StandardLibrary.Web.Scraping
{
    public sealed class StaticScraper : IScraper
    {
        readonly CookieContainer cookies = new CookieContainer();
        readonly WebClient downloadWebClient = new WebClient();

        public event DownloadProgressChangedEventHandler ProgressUpdated;

        public void AddCookie(string url, string name, string content)
        {
            var uri = new Uri(url);
            var cookie = new Cookie(name, content);
            cookies.Add(uri, cookie);
        }

        // Returns a source string scraped from the specified URL.
        public async Task<string> GetSource(string url)
        {
            using (var handler = clientHandler)
            using (var client = getClient(handler))
            {
                // Return the content of the response as a string, or null if 
                // the request was unsuccessful (ex: HTTP 503).
                var resp = await client.GetAsync(url).ConfigureAwait(false);
                return resp.IsSuccessStatusCode
                    ? await resp.Content
                        .ReadAsStringAsync().ConfigureAwait(false)
                    : null;
            }
        }

        // Downloads the content at the specified URL and saves it to the 
        // specified filepath.
        public Task Download(string url, string savePath)
        {
            downloadWebClient.DownloadProgressChanged += ProgressUpdated;
            return downloadWebClient.DownloadFileTaskAsync(url, savePath);
        }

        public void CancelDownload() => downloadWebClient.CancelAsync();

        HttpClientHandler clientHandler 
            => new HttpClientHandler { CookieContainer = cookies, };

        HttpClient getClient(HttpMessageHandler handler)
        {
            // Initialize HTTP client and attach a progress handler.
            var client = new HttpClient(handler);

            // Add headers.
            foreach (var pair in headers)
            {
                client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
            }

            return client;
        }

        IDictionary<string, string> headers
        {
            get
            {
                var userAgent = ScrapeUtils.GetUserAgent(Browser.Chrome);
                return new Dictionary<string, string> 
                { 
                    { "User-Agent", userAgent },
                };
            }
        }
    }
}