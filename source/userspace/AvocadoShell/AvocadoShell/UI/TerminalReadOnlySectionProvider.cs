using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoShell.UI
{
    sealed class TerminalReadOnlySectionProvider : IReadOnlySectionProvider
    {
        public int PromptEndOffset { get; set; }
        public bool IsReadOnly { get; set; }

        public bool CanInsert(int offset)
        {
            return !IsReadOnly && offset >= PromptEndOffset;
        }

        public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
        {
            if (IsReadOnly || segment.EndOffset <= PromptEndOffset)
            {
                return Enumerable.Empty<ISegment>();
            }

            return new[] {
                new TextSegment() {
                    StartOffset = Math.Max(PromptEndOffset, segment.Offset),
                    EndOffset = segment.EndOffset
                }
            };
        }
    }
}
