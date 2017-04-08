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
        GetTags(string tag, params string[] attributeKeys)
        {
            var body = (IHTMLElement2)document.body;
            var elements = body.getElementsByTagName(tag);
            foreach (IHTMLElement elem in elements)
            {
                var dict = attributeKeys.ToDictionary(
                    attr => attr, 
                    attr => (string)Convert.ToString(elem.getAttribute(attr)));
                yield return (elem.innerText, dict);
            }
        }
    }
}