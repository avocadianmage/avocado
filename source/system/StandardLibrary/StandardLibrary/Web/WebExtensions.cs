using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StandardLibrary.Web
{
    public static class WebExtensions
    {
        public static string GetSource(this WebBrowser browser)
        {
            dynamic dom = browser.Document?.DomDocument;
            return dom?.documentElement?.innerHTML;
        }

        public static object RunJavascript(
            this WebBrowser browser, string javascript)
        {
            var doc = browser.Document;
            var head = doc.GetElementsByTagName("head")[0];
            var script = doc.CreateElement("script");
            script.SetAttribute(
                "text", $"function injection() {{ {javascript} }}");
            head.AppendChild(script);
            return browser.Document.InvokeScript("injection");
        }

        public static void Post(
            this WebBrowser browser, 
            string url, 
            Dictionary<string, string> data)
        {
            var pairs = data.Select(p => $"{p.Key}={p.Value}");
            var postData = Encoding.UTF8.GetBytes(string.Join("&", pairs));
            browser.Navigate(url, null, postData, WebConfig.PostHeader);
        }

        public static string BaseUrl(this Uri url)
            => url.GetLeftPart(UriPartial.Authority);
    }
}