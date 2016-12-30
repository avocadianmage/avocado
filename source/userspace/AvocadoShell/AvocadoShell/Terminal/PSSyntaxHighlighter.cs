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
            switch (token.Type)
            {
                case PSTokenType.Variable:
                    return $"${token.Content}" == content;
                case PSTokenType.String:
                    return token.Content.Trim('"') == content.Trim('"');
                default:
                    return token.Content == content;
            }
        }

        readonly object padlock = new object();
        Collection<PSToken> cachedTokens;

        public PSSyntaxHighlighter()
        {
            Reset();
        }

        public void Reset()
        {
            lock (padlock) cachedTokens = new Collection<PSToken>();
        }

        public IDictionary<PSToken, Color?> GetChangedTokens(string text)
        {
            Collection<PSParseError> errors;
            var newTokens = PSParser.Tokenize(text, out errors);

            var deltaTokens = new LinkedList<PSToken>(newTokens);
            Action<Func<int, int>, Func<int, int>> loopTokens = 
                (newTokenIterator, cachedTokenIterator) =>
                {
                    var loopCount = Math.Min(
                        cachedTokens.Count, deltaTokens.Count);
                    for (var i = 0; i < loopCount; i++)
                    {
                        var newToken = newTokens[newTokenIterator(i)];
                        var cachedToken = cachedTokens[cachedTokenIterator(i)];
                        if (compareTokens(newToken, cachedToken))
                            deltaTokens.Remove(newToken);
                    }
                };

            lock (padlock)
            {
                loopTokens(i => i, i => i);
                loopTokens(
                    i => newTokens.Count - i - 1, 
                    i => cachedTokens.Count - i - 1);
                cachedTokens = newTokens;
            }

            return deltaTokens.ToDictionary(
                t => t, t => Config.PSSyntaxColorLookup[t.Type]);
        }

        bool compareTokens(PSToken token1, PSToken token2)
            => token1.Type == token2.Type && token1.Content == token2.Content;
    }
}