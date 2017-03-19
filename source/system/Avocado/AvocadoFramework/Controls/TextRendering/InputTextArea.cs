using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class InputTextArea : TextArea
    {
        const int TAB_SPACING = 4;

        protected Border StylizedCaret { get; private set; }

        public InputTextArea() : base()
        {
            addCommandBindings();
        }

        void addCommandBindings()
        {
            disableFormattingKeys();

            // Ctrl+V, Shift+Ins - Paste.
            this.BindCommand(ApplicationCommands.Paste, OnPaste);
        }

        void disableFormattingKeys() => new ICommand[] {
            EditingCommands.ToggleBold,
            EditingCommands.ToggleItalic,
            EditingCommands.ToggleUnderline,
            EditingCommands.AlignCenter,
            EditingCommands.AlignJustify,
            EditingCommands.AlignLeft,
            EditingCommands.AlignRight
        }.ForEach(c => this.BindCommand(c, () => { }));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            initCaret();
        }

        void initCaret()
        {
            StylizedCaret = this.GetTemplateElement<Border>("Caret");

            // Set size.
            StylizedCaret.Width = CharDimensions.Width + 1;
            StylizedCaret.Height = CharDimensions.Height + 1;

            // RichTextBox events.
            SelectionChanged += (s, e) => updateCaretLocation();
            TextChanged += (s, e) => updateCaretLocation();
            AddHandler(
                ScrollViewer.ScrollChangedEvent,
                new ScrollChangedEventHandler((s, e) => updateCaretLocation()));

            // Window events.
            var window = Window.GetWindow(this);
            window.Activated +=
                (s, e) => StylizedCaret.Visibility = Visibility.Visible;
            window.Deactivated +=
                (s, e) => StylizedCaret.Visibility = Visibility.Collapsed;
        }

        void updateCaretLocation()
        {
            var caretRect = CaretPosition.GetCharacterRect(
                CaretPosition.LogicalDirection);
            Canvas.SetLeft(StylizedCaret, caretRect.X);
            Canvas.SetTop(StylizedCaret, caretRect.Y);
        }

        protected virtual void OnPaste()
        {
            // Disallow other styling when pasting.
            Selection.Text = Clipboard.GetText(TextDataFormat.Text);
            ClearSelection();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Esc: clear the currently selected text.
                case Key.Escape:
                    ClearSelection();
                    break;

                case Key.Tab:
                    e.Handled = processTabKey();
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        bool processTabKey()
        {
            var forward = !WPFUtils.IsShiftKeyDown;

            BeginChange();
            if (Selection.IsEmpty) tab(forward);
            else tabSelection(forward);
            EndChange();

            return true;
        }

        void tab(bool forward)
        {
            var range = new TextRange(LineStartPointer, CaretPosition);
            var offsetX = range.Text.Length;
            if (forward) range.Text += getTabSpaces(offsetX);
            else range = deleteSpaces(offsetX, CaretPosition, false);
            CaretPosition = range.End;
        }

        void tabSelection(bool forward)
        {
            var end = Selection.End;
            end = end.IsAtLineStartPosition
                ? Selection.End : getNextLine(Selection.End);

            var pointer = Selection.Start.GetLineStartPosition(0);
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
            => new string(' ', TAB_SPACING - offsetX % TAB_SPACING);

        int getMaxSpacesToRemove(int offsetX)
        {
            var spaces = offsetX % TAB_SPACING;
            return spaces == 0 ? TAB_SPACING : spaces;
        }

        TextPointer getNextLine(TextPointer pointer)
            => pointer.GetLineStartPosition(1) ?? Document.ContentEnd;
    }
}
