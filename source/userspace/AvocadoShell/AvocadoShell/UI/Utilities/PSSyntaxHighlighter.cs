using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace AvocadoShell.UI.Utilities
{
    sealed class PSSyntaxHighlighter
    {
        Collection<PSToken> cachedTokens;

        public PSSyntaxHighlighter() => Reset();

        public void Reset()
        {
            lock (this) cachedTokens = new Collection<PSToken>();
        }

        delegate T GetIndex<T>(Collection<T> array, int seed);

        IEnumerable<PSToken> getChangedTokens(string text)
        {
            var newTokens = PSParser.Tokenize(text, out var errors);

            var deltaTokens = new LinkedList<PSToken>(newTokens);
            void loopTokens(GetIndex<PSToken> iterator)
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

            return deltaTokens;
        }

        bool compareTokens(PSToken token1, PSToken token2)
            => token1.Type == token2.Type
            && token1.Content == token2.Content
            && token1.Length == token2.Length;

        bool compareTokenToContent(PSToken token, string content)
        {
            var tokenContent = token.Content;
            if (token.Type == PSTokenType.Variable)
            {
                tokenContent = $"${tokenContent}";
            }
            return tokenContent.Trim('"', '\'') == content.Trim('"', '\'');
        }

        public async Task Highlight(TextArea textArea, TextRange range)
        {
            var text = range.Text;
            var tokenization = await Task.Run(() => getChangedTokens(text));
            tokenization.ForEach(t =>
            {
                var foreground = Config.PSSyntaxColorLookup[t.Type];
                textArea.Dispatcher.InvokeAsync(
                    () => applyTokenColoring(textArea, range, t, foreground),
                    DispatcherPriority.ContextIdle);
            });
        }

        void applyTokenColoring(
            TextArea textArea, TextRange range, PSToken token, Brush foreground)
        {
            var start = range.Start.GetPointerFromCharOffset(token.Start);
            if (start == null) return;
            var end = start.GetPointerFromCharOffset(token.Length);
            if (end == null) return;

            var tokenRange = new TextRange(start, end);
            if (!compareTokenToContent(token, tokenRange.Text)) return;

            tokenRange.ApplyPropertyValue(
                TextElement.ForegroundProperty, 
                foreground ?? textArea.Foreground);
        }
    }
}