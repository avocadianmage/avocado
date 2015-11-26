using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace UtilityLib.Web.Scraping.Parsing
{
    public abstract class LinkParser
    {
        protected abstract bool IsValidHref(string data);
        protected abstract string FormatHref(string data);

        public IEnumerable<string> GetLinks(string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);
            return html.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(link => !string.IsNullOrWhiteSpace(link))
                .Where(link => IsValidHref(link))
                .Select(link => FormatHref(link));
        }
    }
}