using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AvocadoFramework.Controls.Text.Input
{
    public abstract class InputControl : TextControl
    {
        readonly CaretUI caret = new CaretUI();

        public static readonly DependencyProperty InputBrushProperty
            = DependencyProperty.Register(
                "InputBrush",
                typeof(Brush),
                typeof(InputControl),
                new FrameworkPropertyMetadata(Brushes.White));

        public Brush InputBrush
        {
            get { return GetValue(InputBrushProperty) as Brush; }
            set { SetValue(InputBrushProperty, value); }
        }

        protected bool InputEnabled { get; set; }

        protected bool IsControlKeyDown
        {
            get { return Keyboard.Modifiers.HasFlag(ModifierKeys.Control); }
        }

        protected bool IsShiftKeyDown
        {
            get { return Keyboard.Modifiers.HasFlag(ModifierKeys.Shift); }
        }

        //ckgtest: move to onapplytemplate
        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);

            TextContent.CaretMoved += onCaretMoved;

            Focus();
        }

        void onCaretMoved(object sender, CaretMovedEventArgs e)
        {
            caret.SetPosition(e.ParentTextLine, e.GridX);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Do not recognize any key presses if input is disabled.
            if (!InputEnabled) return;

            processSpecialKeyInput(e);
        }

        void processSpecialKeyInput(KeyEventArgs e)
        {
            // Check if any modifiers are pressed.
            switch (e.Key)
            {
                case Key.Back:
                    OnBackKeyDown(e);
                    e.Handled = true;
                    break;
                case Key.Delete:
                    OnDeleteKeyDown(e);
                    break;
                case Key.Enter:
                    OnEnterKeyDown(e);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    OnEscapeKeyDown(e);
                    e.Handled = true;
                    break;
                case Key.Tab:
                    OnTabKeyDown(e);
                    break;
                case Key.Left:
                    OnLeftKeyDown(e);
                    break;
                case Key.Right:
                    OnRightKeyDown(e);
                    break;
                case Key.Up:
                    OnUpKeyDown(e);
                    break;
                case Key.Down:
                    OnDownKeyDown(e);
                    break;
                case Key.Home:
                    OnHomeKeyDown(e);
                    break;
                case Key.End:
                    OnEndKeyDown(e);
                    break;
                case Key.V:
                    if (!IsControlKeyDown) break;
                    OnCtrlVKeyDown(e);
                    break;
            }
        }

        // Handle keyboard input of text characters.
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            // Do not recognize any key presses if input is disabled.
            if (!InputEnabled) return;

            WriteInput(e.Text);
        }

        protected virtual void OnBackKeyDown(KeyEventArgs e)
        {
            // Quit if the caret is at the start of the document.
            if (TextContent.CaretAtDocumentStart) return;

            // Delete the previous segment.
            TextContent.Translate(-1);
            TextContent.DeleteSegments(1);
        }

        protected virtual void OnDeleteKeyDown(KeyEventArgs e)
        {
            // Delete the current segment.
            TextContent.DeleteSegments(1);
        }

        protected virtual void OnEnterKeyDown(KeyEventArgs e)
        {
            TextContent.InsertLineBreak();
        }

        protected virtual void OnEscapeKeyDown(KeyEventArgs e)
        {
            // Base implementation does nothing.
        }

        protected virtual void OnTabKeyDown(KeyEventArgs e)
        {
            if (IsShiftKeyDown) deleteTab();
            else insertTab();
        }

        protected virtual void OnCtrlVKeyDown(KeyEventArgs e)
        {
            // Paste the text from the clipboard.
            WriteInput(Clipboard.GetText());
        }

        void insertTab()
        {
            var spaceCount
                = Config.TabWidth - TextContent.CaretX % Config.TabWidth;
            var tabString = new string(' ', spaceCount);
            WriteInput(tabString);
        }

        void deleteTab()
        {
            // Quit if we are at the start of the line.
            if (TextContent.CaretX == 0) return;

            // Get the maximum possible amount of segments we can delete.
            var maxSegmentsToDelete = TextContent.CaretX % Config.TabWidth;
            if (maxSegmentsToDelete == 0)
            {
                maxSegmentsToDelete = Config.TabWidth;
            }

            // Calculate the exact number of segments we can delete.
            var segmentsToDelete = 0;
            var lineStr = TextContent.GetCurrentLineText();
            for (var i = 1; i <= maxSegmentsToDelete; i++)
            {
                // Ensure we are only deleting segments that are space
                // characters.
                var c = lineStr[TextContent.CaretX - i];
                if (c != ' ') break;
                segmentsToDelete++;
            }

            // Perform the deletion.
            TextContent.Translate(-segmentsToDelete);
            TextContent.DeleteSegments(segmentsToDelete);
        }

        protected virtual void OnLeftKeyDown(KeyEventArgs e)
        {
            TextContent.Translate(-1);
        }

        protected virtual void OnRightKeyDown(KeyEventArgs e)
        {
            TextContent.Translate(1);
        }

        protected virtual void OnUpKeyDown(KeyEventArgs e)
        {
            TextContent.TranslateToPrevLine();
        }

        protected virtual void OnDownKeyDown(KeyEventArgs e)
        {
            TextContent.TranslateToNextLine();
        }

        protected virtual void OnHomeKeyDown(KeyEventArgs e)
        {
            if (IsControlKeyDown) TextContent.TranslateToDocumentStart();
            else TextContent.TranslateToLineStart();
        }

        protected virtual void OnEndKeyDown(KeyEventArgs e)
        {
            if (IsControlKeyDown) TextContent.TranslateToDocumentEnd();
            TextContent.TranslateToLineEnd();
        }

        protected void WriteInput(string text)
        {
            // Write out each character as an individual segment.
            TextContent.InsertText(text, InputBrush, true);
        }
    }
}
