using System.Linq;
using System.Windows.Documents;
using static AvocadoFramework.Controls.TextRendering.TextConfig;

namespace AvocadoFramework.Controls.TextRendering.Input
{
    sealed class TabSpacer
    {
        readonly TextPointer caretPosition;
        readonly TextSelection selection;

        public TabSpacer(TextPointer caretPosition, TextSelection selection)
        {
            this.caretPosition = caretPosition;
            this.selection = selection;
        }

        public TextPointer Run(bool forward)
        {
            if (selection.IsEmpty) return tab(forward);
            else
            {
                tabSelection(forward);
                return null;
            }
        }

        TextPointer tab(bool forward)
        {
            var lineStart = caretPosition.GetLineStartPosition(0);
            var range = new TextRange(lineStart, caretPosition);
            var offsetX = range.Text.Length;
            if (forward) range.Text += getTabSpaces(offsetX);
            else range = deleteSpaces(offsetX, caretPosition, false);
            return range.End;
        }

        void tabSelection(bool forward)
        {
            var end = selection.End;
            end = end.IsAtLineStartPosition
                ? selection.End : getNextLine(selection.End);

            var pointer = selection.Start.GetLineStartPosition(0);
            do
            {
                tabSelectedLine(pointer, forward);
                pointer = getNextLine(pointer);
            }
            while (new TextRange(pointer, end).Text.Length > 0);
        }

        void tabSelectedLine(TextPointer lineStart, bool forward)
        {
            var range = new TextRange(lineStart, getNextLine(lineStart));
            var offsetX = range.Text.TakeWhile(c => c == ' ').Count();

            if (forward) lineStart.InsertTextInRun(getTabSpaces(offsetX));
            else deleteSpaces(offsetX, lineStart, true);
        }

        TextRange deleteSpaces(
            int offsetX, TextPointer pointer, bool lookForward)
        {
            var maxSpacesToRemove = getMaxSpacesToRemove(offsetX);
            var range = new TextRange(pointer, pointer);
            while (range.Text.Length < maxSpacesToRemove)
            {
                var curPointer = lookForward ? range.End : range.Start;
                var direction = lookForward
                    ? LogicalDirection.Forward : LogicalDirection.Backward;
                var nextPos = curPointer.GetNextInsertionPosition(direction);
                if (nextPos == null) break;
                range = lookForward
                    ? new TextRange(range.Start, nextPos)
                    : new TextRange(nextPos, range.End);
            }
            range.Text = lookForward
                ? range.Text.TrimStart(' ') : range.Text.TrimEnd(' ');
            return range;
        }

        string getTabSpaces(int offsetX)
            => new string(' ', TabSpacing - offsetX % TabSpacing);

        int getMaxSpacesToRemove(int offsetX)
        {
            var spaces = offsetX % TabSpacing;
            return spaces == 0 ? TabSpacing : spaces;
        }

        TextPointer getNextLine(TextPointer pointer)
            => pointer.GetLineStartPosition(1) ?? pointer.DocumentEnd;
    }
}
