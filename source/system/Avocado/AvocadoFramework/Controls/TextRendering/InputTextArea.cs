using System.Windows;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering
{
    public class InputTextArea : TextArea
    {
        protected bool InputEnabled { get; set; }

        protected bool IsControlKeyDown
            => Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        protected bool IsShiftKeyDown
            => Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);
            TextBase.Focus();
        }
    }
}
