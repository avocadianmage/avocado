using System.Windows;
using System.Windows.Media;

namespace AvocadoFramework
{
    sealed class Config
    {
        // Window opacity properties.
        public static double WindowFadeDuration { get; } = 250;

        // Window border properties.
        public static Color InactiveBorderColor { get; } = Colors.Gray;
        public static Color ActiveBorderColor { get; } = Colors.CornflowerBlue;
        public static double BorderFadeDuration { get; } = 500;

        // Text properties.
        public static double TextFadeDuration { get; } = 150;
        public static Typeface Typeface
        {
            get
            {
                return new Typeface(
                    new FontFamily("Consolas"),
                    FontStyles.Normal,
                    FontWeights.Bold,
                    FontStretches.Normal);
            }
        }
        public static double FontSize { get; } = 10;
        public static int TabWidth { get; } = 4;

        // Progressor properties.
        public static Color ProgressorColor { get; } = Colors.Green;
        public static Color ProgressorSelectedColor { get; } = Colors.Orange;
        public static double ProgressorFadeDuration { get; } = 200;
    }
}