using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AvocadoShell.UI.Utilities
{
    sealed class PSSyntaxHighlighter
    {
        CancellationTokenSource cancelTokenSource;
        Token[] cachedTokens;

        public PSSyntaxHighlighter() => Reset();

        public void Reset()
        {
            lock (this) cachedTokens = new Token[0];
        }

        delegate T GetIndex<T>(T[] array, int seed);

        IEnumerable<Token> getChangedTokens(string text)
        {
            Parser.ParseInput(text, out var newTokens, out var errors);
            
            var deltaTokens = new LinkedList<Token>(newTokens);
            void loopTokens(GetIndex<Token> iterator)
            {
                var loopCount = Math.Min(
                    cachedTokens.Length, deltaTokens.Count);
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
                loopTokens((array, seed) => array[array.Length - seed - 1]);
                cachedTokens = newTokens;
            }

            return deltaTokens;
        }

        bool compareTokens(Token token1, Token token2)
            => token1.Kind == token2.Kind 
            && token1.TokenFlags == token2.TokenFlags 
            && token1.Text == token2.Text;

        public async Task Highlight(TextArea textArea, TextRange range)
        {
            var text = range.Text;
            var changedTokens = await Task.Run(() => getChangedTokens(text));
            if (!changedTokens.Any()) return;

            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            var start = range.Start;
            await Task.Run(() =>
            {
                var cancelToken = cancelTokenSource.Token;
                changedTokens.ForEach(t =>
                {
                    textArea.Dispatcher.InvokeAsync(() => 
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        applyTokenColoring(textArea, ref start, t);
                    }, Config.TextPriority);
                });
            }, cancelTokenSource.Token);
        }

        void applyTokenColoring(
            TextArea textArea, ref TextPointer baseStart, Token token)
        {
            var tokenStartOffset = token.Extent.StartOffset;
            var tokenStart = baseStart.GetPointerFromCharOffset(
                tokenStartOffset);
            if (tokenStart == null) return;
            var tokenEnd = tokenStart.GetPointerFromCharOffset(
                token.Text.Length);
            if (tokenEnd == null) return;

            var tokenRange = new TextRange(tokenStart, tokenEnd);
            var tokenText = tokenRange.Text;
            var caretIndexInRange
                = textArea.CaretPosition.GetOffsetInRange(tokenRange);

            textArea.BeginChange(); try
            {
                // Delete previous text and insert a stylized run. This is done 
                // instead of TextRange.ApplyPropertyValue due to performance.
                tokenRange.Text = string.Empty;
                var run = new Run(tokenText, tokenStart)
                {
                    Foreground = Config.GetTokenBrush(token)
                        ?? textArea.Foreground
                };

                // If the caret was within the original range of the token, 
                // reposition within the next text.
                if (caretIndexInRange >= 0)
                {
                    textArea.CaretPosition = run.ContentStart
                        .GetPointerFromCharOffset(caretIndexInRange);
                }

                // If replacing the first token in the sequence, make sure the 
                // original position of baseStart is preserved.
                if (tokenStartOffset == 0) baseStart = run.ContentStart;

                // If the token is an expandable string, highlight any nested
                // tokens (ex: embedded variables).
                if (token is StringExpandableToken stringToken)
                {
                    var nestedTokens = stringToken.NestedTokens;
                    if (nestedTokens == null) return;
                    foreach (var nestedToken in nestedTokens)
                    {
                        applyTokenColoring(
                            textArea, ref baseStart, nestedToken);
                    }
                }
            }
            finally { textArea.EndChange(); }
        }
    }
}