using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Windows.Media;

namespace AvocadoShell.Terminal
{
    sealed class PSSyntaxHighlighter
    {
        public static bool CompareTokenToContent(PSToken token, string content)
        {
            var tokenContent = token.Content;
            if (token.Type == PSTokenType.Variable)
                tokenContent = $"${tokenContent}";
            return tokenContent.Trim('"', '\'') == content.Trim('"', '\'');
        }

        Collection<PSToken> cachedTokens;

        public PSSyntaxHighlighter()
        {
            Reset();
        }

        public void Reset()
        {
            lock (this) cachedTokens = new Collection<PSToken>();
        }

        delegate T GetIndex<T>(Collection<T> array, int seed);

        public IDictionary<PSToken, Brush> GetChangedTokens(string text)
        {
            Collection<PSParseError> errors;
            var newTokens = PSParser.Tokenize(text, out errors);

            var deltaTokens = new LinkedList<PSToken>(newTokens);
            Action<GetIndex<PSToken>> loopTokens = (iterator) =>
            {
                var loopCount = Math.Min(
                    cachedTokens.Count, deltaTokens.Count);
                for (var i = 0; i < loopCount; i++)
                {
                    var newToken = iterator(newTokens, i);
                    var cachedToken = iterator(cachedTokens, i);
                    if (!compareTokens(newToken, cachedToken)) break;
                    deltaTokens.Remove(newToken);
                }
            };

            lock (this)
            {
                loopTokens((array, seed) => array[seed]);
                loopTokens((array, seed) => array[array.Count - seed - 1]);
                cachedTokens = newTokens;
            }

            return deltaTokens.ToDictionary(
                t => t, t => Config.PSSyntaxColorLookup[t.Type]);
        }

        bool compareTokens(PSToken token1, PSToken token2)
            => token1.Type == token2.Type
            && token1.Content == token2.Content
            && token1.Length == token2.Length;
    }
}