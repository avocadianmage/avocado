using System.Windows;
using System.Windows.Media;

namespace AvocadoFramework
{
    sealed class Config
    {
        // Window opacity properties.
        public static double MinOpacity { get { return 0; } }
        public static double MaxOpacity { get { return 0.8; } }
        public static double WindowFadeDuration { get { return 250; } }

        // Window border properties.
        public static Color InactiveBorderColor { get { return Colors.Gray; } }
        public static Color ActiveBorderColor 
        { 
            get { return Colors.LightGreen; } 
        }
        public static double BorderFadeDuration { get { return 500; } }

        // Text properties.
        public static double TextFadeDuration { get { return 150; } }
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
        public static double FontSize { get { return 10; } }
        public static int TabWidth { get { return 4; } }

        // Caret properties.
        public static Color CaretColor { get { return Colors.Orange; } }
        public static int CaretThickness { get { return 1; } }
        public static double CaretFadeDuration { get { return 750; } }

        // Progressor properties.
        public static Color ProgressorColor { get { return Colors.Green; } }
        public static Color ProgressorSelectedColor 
        { 
            get { return Colors.Orange; } 
        }
        public static double ProgressorFadeDuration { get { return 200; } }
    }
}