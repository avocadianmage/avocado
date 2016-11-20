using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class InputTextArea : TextArea
    {
        protected void EnableInput(bool enabled) => inputEnabled = enabled;

        bool inputEnabled = false;

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
            TextBase.SelectionChanged += (s, e) => updateCaretLocation(caret);
            TextBase.TextChanged += (s, e) => updateCaretLocation(caret);
            var handler = new ScrollChangedEventHandler(
                (s, e) => updateCaretLocation(caret));
            TextBase.AddHandler(ScrollViewer.ScrollChangedEvent, handler);

            // Window events.
            var window = Window.GetWindow(this);
            window.Activated += 
                (s, e) => caret.Visibility = Visibility.Visible;
            window.Deactivated += 
                (s, e) => caret.Visibility = Visibility.Collapsed;
        }

        void updateCaretLocation(UIElement caret)
        {
            var caretRect = CaretPointer.GetCharacterRect(
                CaretPointer.LogicalDirection);
            Canvas.SetLeft(caret, caretRect.X);
            Canvas.SetTop(caret, caretRect.Y);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Ignore input if the InputEnabled flag is false.
            // The exception to this is system key handling (ex: Alt+F4).
            if (!inputEnabled && e.Key != Key.System)
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
                // Sanitize linebreak.
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
