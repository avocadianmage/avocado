using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows.Media;

sealed class StaticColorizer : DocumentColorizingTransformer
{
    readonly Dictionary<int, List<(int offset, int endOffset, Brush foreground)>> coloredLineParts 
        = new Dictionary<int, List<(int offset, int endOffset, Brush foreground)>>();

    public void AddColoredLinePart(
        int lineNumber, int offset, int endOffset, Brush foreground)
    {
        if (!coloredLineParts.ContainsKey(lineNumber))
        {
            coloredLineParts[lineNumber] 
                = new List<(int offset, int endOffset, Brush foreground)>();
        }

        coloredLineParts[lineNumber].Add((offset, endOffset, foreground));
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.IsDeleted || !coloredLineParts.ContainsKey(line.LineNumber))
        {
            return;
        }

        coloredLineParts[line.LineNumber].ForEach(part =>
        {
            ChangeLinePart(part.offset, part.endOffset, element =>
            {
                element.TextRunProperties.SetForegroundBrush(part.foreground);
            });
        });
    }
}