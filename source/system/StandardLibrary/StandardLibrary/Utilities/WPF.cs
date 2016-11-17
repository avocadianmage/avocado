using System.Windows.Input;

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
    }
}