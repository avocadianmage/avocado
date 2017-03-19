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
            if (Selection.IsEmpty) insertTab();
            else
            {
                var end = getNextLine(Selection.End);
                var pointer = Selection.Start.GetLineStartPosition(0);
                do
                {
                    tabSelectedLine(pointer);
                    pointer = getNextLine(pointer);
                }
                while (pointer.GetOffsetToPosition(end) > 0);
            }
            return true;
        }

        void insertTab()
        {
            var range = new TextRange(LineStartPointer, CaretPosition);
            Write(getTabSpaces(range.Text.Length), Foreground);
        }

        void tabSelectedLine(TextPointer lineStart)
        {
            var range = new TextRange(lineStart, getNextLine(lineStart));
            var caretPosX = range.Text.TakeWhile(c => c == ' ').Count();
            lineStart.InsertTextInRun(getTabSpaces(caretPosX));
        }

        string getTabSpaces(int x) 
            => new string(' ', TAB_SPACING - x % TAB_SPACING);

        TextPointer getNextLine(TextPointer pointer)
            => pointer.GetLineStartPosition(1) ?? Document.ContentEnd;
    }
}
