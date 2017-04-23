﻿using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AvocadoShell.UI.Utilities
{
    sealed class PSSyntaxHighlighter
    {
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
            var tokenization = await Task.Run(() => getChangedTokens(text));
            tokenization.ForEach(t =>
            {
                textArea.Dispatcher.InvokeAsync(
                    () => applyTokenColoring(textArea, range.Start, t),
                    Config.TextPriority);
            });
        }

        void applyTokenColoring(
            TextArea textArea, TextPointer baseStart, Token token)
        {
            var tokenStart = baseStart.GetPointerFromCharOffset(
                token.Extent.StartOffset);
            if (tokenStart == null) return;
            var tokenEnd = tokenStart.GetPointerFromCharOffset(
                token.Text.Length);
            if (tokenEnd == null) return;

            var tokenRange = new TextRange(tokenStart, tokenEnd);
            var tokenText = tokenRange.Text;
            var caretIndexInRange 
                = textArea.CaretPosition.GetOffsetInRange(tokenRange);

            textArea.BeginChange();

            // Delete previous text and insert a stylized run. This is done 
            // instead of TextRange.ApplyPropertyValue due to performance 
            // reasons.
            tokenRange.Text = string.Empty;
            var run = new Run(tokenText, tokenStart)
            {
                Foreground = Config.GetTokenBrush(token) ?? textArea.Foreground
            };

            // If the caret was within the original range of the token, 
            // reposition within the next text.
            if (caretIndexInRange >= 0)
            {
                textArea.CaretPosition = run.ContentStart
                    .GetPointerFromCharOffset(caretIndexInRange);
            }

            // If the token is an expandable string, highlight any nested tokens
            // (ex: embedded variables).
            if (token is StringExpandableToken stringToken)
            {
                stringToken.NestedTokens?.ForEach(
                    t => applyTokenColoring(textArea, baseStart, t));
            }

            textArea.EndChange();
        }
    }
}