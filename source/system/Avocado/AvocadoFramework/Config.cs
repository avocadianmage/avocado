using System.Windows.Media;

namespace AvocadoFramework
{
    sealed class Config
    {
        // Window opacity properties.
        public static double WindowFadeDuration => 200;

        // Window border properties.
        public static Color InactiveBorderColor => Colors.Gray;
        public static Color ActiveBorderColor { get; } 
            = Color.FromRgb(0, 122, 204);
        public static Color ActiveBorderColorElevated => Colors.Salmon;
        public static double BorderFadeDuration => 500;

        // Text properties.
        public static double TextFadeDuration => 200;

        // Caret properties.
        public static Brush CaretBrush => Brushes.Orange;
        public static double CaretBlinkDuration => 750;

        // Progressor properties.
        public static Color ProgressorColor => Colors.Green;
        public static Color ProgressorSelectedColor => Colors.Orange;
        public static double ProgressorFadeDuration => 200;
    }
}