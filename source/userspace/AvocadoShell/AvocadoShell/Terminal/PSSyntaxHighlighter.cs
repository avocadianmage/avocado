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
        static Dictionary<PSTokenType, Color?> ColorLookup
            = new Dictionary<PSTokenType, Color?>
            {
                { PSTokenType.Attribute, Color.FromRgb(230, 168, 255) },
                { PSTokenType.Command, Color.FromRgb(78, 201, 176) },
                { PSTokenType.CommandArgument, null },
                { PSTokenType.CommandParameter, Colors.SkyBlue },
                { PSTokenType.Comment, Colors.DimGray },
                { PSTokenType.GroupStart, null },
                { PSTokenType.GroupEnd, null },
                { PSTokenType.Keyword, Color.FromRgb(86, 156, 214) },
                { PSTokenType.LineContinuation, Colors.Orange },
                { PSTokenType.Member, null },
                { PSTokenType.NewLine, null },
                { PSTokenType.Number, Color.FromRgb(184, 215, 163) },
                { PSTokenType.Operator, null },
                { PSTokenType.StatementSeparator, Colors.Orange },
                { PSTokenType.String, Colors.Yellow },
                { PSTokenType.Type, Color.FromRgb(230, 168, 255) },
                { PSTokenType.Variable, Color.FromRgb(184, 215, 163) },
                { PSTokenType.Unknown, null }
            };

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

            return deltaTokens.ToDictionary(t => t, t => ColorLookup[t.Type]);
        }

        bool compareTokens(PSToken token1, PSToken token2)
            => token1.Type == token2.Type && token1.Content == token2.Content;
    }
}