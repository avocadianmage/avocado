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
            var isShiftKeyDown = WPFUtils.IsShiftKeyDown;

            BeginChange();
            if (Selection.IsEmpty)
            {
                if (isShiftKeyDown) removeTab();
                else insertTab();
            }
            else
            {
                var end = Selection.End;
                end = end.IsAtLineStartPosition
                    ? Selection.End : getNextLine(Selection.End);

                var pointer = Selection.Start.GetLineStartPosition(0);
                do
                {
                    tabSelectedLine(pointer, !isShiftKeyDown);
                    pointer = getNextLine(pointer);
                }
                while (new TextRange(pointer, end).Text.Length > 0);
            }
            EndChange();

            return true;
        }

        void insertTab()
        {
            var range = new TextRange(LineStartPointer, CaretPosition);
            range.Text += getTabSpaces(range.Text.Length);
            CaretPosition = range.End;
        }

        void removeTab()
        {
            var range = new TextRange(LineStartPointer, CaretPosition);

            var maxSpacesToRemove = getMaxSpacesToRemove(range.Text.Length);
            var rangeToRemove = new TextRange(CaretPosition, CaretPosition);
            while (rangeToRemove.Text.Length < maxSpacesToRemove)
            {
                var prevPos = rangeToRemove.Start.GetNextInsertionPosition(
                    LogicalDirection.Backward);
                if (prevPos == null) break;
                rangeToRemove = new TextRange(prevPos, rangeToRemove.End);
            }
            rangeToRemove.Text = rangeToRemove.Text.TrimEnd(' ');

            CaretPosition = rangeToRemove.End;
        }

        void tabSelectedLine(TextPointer lineStart, bool forward)
        {
            var range = new TextRange(lineStart, getNextLine(lineStart));
            var caretPosX = range.Text.TakeWhile(c => c == ' ').Count();

            if (forward) lineStart.InsertTextInRun(getTabSpaces(caretPosX));
            else
            {
                var maxSpacesToRemove = getMaxSpacesToRemove(caretPosX);
                var rangeToRemove = new TextRange(lineStart, lineStart);
                while (rangeToRemove.Text.Length < maxSpacesToRemove)
                {
                    var nextPos = rangeToRemove.End.GetNextInsertionPosition(
                        LogicalDirection.Forward);
                    if (nextPos == null) break;
                    rangeToRemove = new TextRange(rangeToRemove.Start, nextPos);
                }
                rangeToRemove.Text = rangeToRemove.Text.TrimStart(' ');
            }
        }

        string getTabSpaces(int x) 
            => new string(' ', TAB_SPACING - x % TAB_SPACING);

        int getMaxSpacesToRemove(int x)
        {
            var spaces = x % TAB_SPACING;
            return spaces == 0 ? TAB_SPACING : spaces;
        }

        TextPointer getNextLine(TextPointer pointer)
            => pointer.GetLineStartPosition(1) ?? Document.ContentEnd;
    }
}
