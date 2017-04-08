using mshtml;
using System.Collections.Generic;

namespace StandardLibrary.Web
{
    public sealed class HTMLParser
    {
        readonly IHTMLDocument2 document = (IHTMLDocument2)new HTMLDocument();

        public HTMLParser(string source) => document.write(source);

        public IEnumerable<(string url, string text)> GetLinks()
        {
            var body = (IHTMLElement2)document.body;
            var links = body.getElementsByTagName("a");
            foreach (IHTMLElement link in links)
            {
                yield return (link.getAttribute("href"), link.innerText);
            }
        }
    }
}