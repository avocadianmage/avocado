using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StandardLibrary.Web
{
    public sealed class HTMLParser
    {
        readonly IHTMLDocument2 document = (IHTMLDocument2)new HTMLDocument();

        public HTMLParser(string source) => document.write(source);

        public IEnumerable<(
            string inner, 
            Dictionary<string, string> attributes)> 
        GetLinks(params string[] attributeKeys)
        {
            var body = (IHTMLElement2)document.body;
            var links = body.getElementsByTagName("a");
            foreach (IHTMLElement link in links)
            {
                var dict = attributeKeys.ToDictionary(
                    attr => attr, 
                    attr => (string)Convert.ToString(link.getAttribute(attr)));
                yield return (link.innerText, dict);
            }
        }
    }
}