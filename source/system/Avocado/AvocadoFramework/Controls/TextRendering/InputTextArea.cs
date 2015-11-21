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
                Write(Clipboard.GetText(TextDataFormat.Text), Foreground);
            }
        }
    }
}
