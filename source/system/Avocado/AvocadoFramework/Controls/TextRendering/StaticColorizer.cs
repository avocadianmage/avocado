using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows.Media;

sealed class StaticColorizer : DocumentColorizingTransformer
{
    readonly Dictionary<int, LinkedList<(
                                int offset, 
                                int endOffset, 
                                Brush foreground)>> 
        coloredLineParts = new Dictionary<int, LinkedList<(
                                int offset, 
                                int endOffset, 
                                Brush foreground)>>();

    public void AddColoredLinePart(
        int lineNumber, int offset, int endOffset, Brush foreground)
    {
        if (!coloredLineParts.ContainsKey(lineNumber))
        {
            coloredLineParts[lineNumber]
                = new LinkedList<(int offset, int endOffset, Brush foreground)>();
        }

        coloredLineParts[lineNumber].AddLast((offset, endOffset, foreground));
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.IsDeleted || !coloredLineParts.ContainsKey(line.LineNumber))
        {
            return;
        }

        var currentLinePart = coloredLineParts[line.LineNumber].First;
        while (currentLinePart != null)
        {
            var val = currentLinePart.Value;
            ChangeLinePart(val.offset, val.endOffset, element =>
            {
                element.TextRunProperties.SetForegroundBrush(val.foreground);
            });
            currentLinePart = currentLinePart.Next;
        }
    }
}