using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;

public class TerminalReadOnlySectionProvider : IReadOnlySectionProvider
{
    public int EndOffset { get; set; }
    public bool IsReadOnly { get; set; }

    public bool CanInsert(int offset) => !IsReadOnly && offset >= EndOffset;

    public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
    {
        if (segment.EndOffset <= EndOffset) return Enumerable.Empty<ISegment>();

        return new[] {
            new TextSegment() {
                StartOffset = Math.Max(EndOffset, segment.Offset),
                EndOffset = segment.EndOffset
            }
        };
    }
}