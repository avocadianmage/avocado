using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class InputTextArea : TextArea
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            initCaret();
        }

        void initCaret()
        {
            var caret = this.GetTemplateElement<Border>("Caret");

            // Set size.
            caret.Width = CharDimensions.Width + 1;
            caret.Height = CharDimensions.Height + 1;

            // RichTextBox events.
            SelectionChanged += (s, e) => updateCaretLocation(caret);
            TextChanged += (s, e) => updateCaretLocation(caret);
            var handler = new ScrollChangedEventHandler(
                (s, e) => updateCaretLocation(caret));
            AddHandler(ScrollViewer.ScrollChangedEvent, handler);

            // Window events.
            var window = Window.GetWindow(this);
            window.Activated += 
                (s, e) => caret.Visibility = Visibility.Visible;
            window.Deactivated += 
                (s, e) => caret.Visibility = Visibility.Collapsed;
        }

        void updateCaretLocation(UIElement caret)
        {
            var caretRect = CaretPosition.GetCharacterRect(
                CaretPosition.LogicalDirection);
            Canvas.SetLeft(caret, caretRect.X);
            Canvas.SetTop(caret, caretRect.Y);
        }

        protected void ClearUndoBuffer()
        {
            IsUndoEnabled = false;
            IsUndoEnabled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Ignore input if the InputEnabled flag is false.
            // The exception to this is system key handling (ex: Alt+F4).
            if (IsReadOnly && e.Key != Key.System)
            {
                e.Handled = true;
                return;
            }

            // Handle any special key actions.
            HandleSpecialKeys(e);

            // Handle all other keys.
            base.OnPreviewKeyDown(e);
        }

        protected virtual void HandleSpecialKeys(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Format linebreak.
                case Key.Enter:
                    WriteLine();
                    e.Handled = true;
                    break;
                    
                // Disallow other styling when pasting.
                case Key.V:
                    if (!WPF.IsControlKeyDown) break;
                    Write(Clipboard.GetText(TextDataFormat.Text), Foreground);
                    e.Handled = true;
                    break;
            }
        }
    }
}
