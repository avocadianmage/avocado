using System.Windows.Media;

namespace AvocadoFramework
{
    sealed class Config
    {
        // Text properties.
        public static double TextFadeDuration => 200;

        // Caret properties.
        public static Brush CaretBrush => Brushes.Orange;
        public static double CaretBlinkDuration => 750;
    }
}