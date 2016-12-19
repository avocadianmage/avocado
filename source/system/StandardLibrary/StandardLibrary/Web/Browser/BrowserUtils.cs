using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StandardLibrary.Web.Browser
{
    static class BrowserUtils
    {
        public static string GetSource(this WebBrowser browser)
        {
            dynamic dom = browser.Document?.DomDocument;
            return dom?.documentElement?.innerHTML;
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
    }
}