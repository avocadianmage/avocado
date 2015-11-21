using System;
using System.Windows;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering
{
    public class InputTextArea : TextArea
    {
        protected bool IsControlKeyDown
            => Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        protected bool IsShiftKeyDown
            => Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);
            TextBase.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Disallow other styling when pasting.
            if (IsControlKeyDown && e.Key == Key.V)
            {
                e.Handled = true;
                paste();
            }
        }

        void paste()
        {
            // Change any paragraph breaks to linebreaks.
            var text = Clipboard.GetText(TextDataFormat.Text)
                .Replace(Environment.NewLine, "\r");
            Write(text, Foreground);
        }
    }
}
