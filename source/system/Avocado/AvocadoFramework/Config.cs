using System.Windows.Media;

namespace AvocadoFramework
{
    sealed class Config
    {
        // Window opacity properties.
        public static double WindowFadeDuration => 250;

        // Window border properties.
        public static Color InactiveBorderColor => Colors.Gray;
        public static Color ActiveBorderColor => Colors.CornflowerBlue;
        public static Color ActiveBorderColorElevated => Colors.Salmon;
        public static double BorderFadeDuration => 500;

        // Text properties.
        public static double TextFadeDuration => 200;
        public static Brush CaretBrush => Brushes.Orange;
        public static double CursorBlinkDuration => 750;

        // Progressor properties.
        public static Color ProgressorColor => Colors.Green;
        public static Color ProgressorSelectedColor => Colors.Orange;
        public static double ProgressorFadeDuration => 200;
    }
}