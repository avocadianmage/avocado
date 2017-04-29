using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StandardLibrary.Web.Scraping
{
    public sealed class StaticScraper
    {
        readonly CookieContainer cookies = new CookieContainer();

        public void AddCookie(string url, string name, string content) =>
            cookies.Add(new Uri(url), new Cookie(name, content));

        // Returns a source string scraped from the specified URL.
        public async Task<string> GetSource(string url)
        {
            using (var handler = clientHandler)
            using (var client = getClient(handler))
            {
                // Return the content of the response as a string, or null if 
                // the request was unsuccessful (ex: HTTP 503).
                var resp = await client.GetAsync(url);
                return resp.IsSuccessStatusCode
                    ? await resp.Content.ReadAsStringAsync() : null;
            }
        }

        HttpClientHandler clientHandler 
            => new HttpClientHandler { CookieContainer = cookies };

        HttpClient getClient(HttpMessageHandler handler)
        {
            // Initialize HTTP client and attach a progress handler.
            var client = new HttpClient(handler);

            // Add headers.
            headers.ForEach(
                p => client.DefaultRequestHeaders.Add(p.Key, p.Value));

            return client;
        }

        IDictionary<string, string> headers => new Dictionary<string, string>
        {
            { "User-Agent", WebConfig.UserAgentChrome }
        };
    }
}