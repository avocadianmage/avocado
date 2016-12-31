using System.Windows.Input;
using System.Windows.Media;

namespace StandardLibrary.Utilities
{
    public static class WPF
    {
        public static bool IsAltKeyDown
            => isModifierKeyDown(ModifierKeys.Alt);

        public static bool IsControlKeyDown
            => isModifierKeyDown(ModifierKeys.Control);

        public static bool IsShiftKeyDown
            => isModifierKeyDown(ModifierKeys.Shift);

        static bool isModifierKeyDown(ModifierKeys key)
            => Keyboard.Modifiers.HasFlag(key);

        public static SolidColorBrush CreateBrush(byte r, byte g, byte b)
            => CreateBrush(Color.FromRgb(r, g, b));

        public static SolidColorBrush CreateBrush(Color color) 
            => CreateBrush(color, 1);

        public static SolidColorBrush CreateBrush(
            byte r, byte g, byte b, double opacity)
            => CreateBrush(Color.FromRgb(r, g, b), opacity);

        public static SolidColorBrush CreateBrush(Color color, double opacity)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();
            return brush;
        }
    }
}