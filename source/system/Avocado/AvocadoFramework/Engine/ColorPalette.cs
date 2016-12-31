using System.Windows.Media;
using static StandardLibrary.Utilities.WPF;

namespace AvocadoFramework.Engine
{
    public static class ColorPalette
    {
        public static SolidColorBrush LightGray => Brushes.LightGray;
        public static SolidColorBrush DarkGray => Brushes.DimGray;

        public static SolidColorBrush LightRed => Brushes.Salmon;
        public static SolidColorBrush Orange => Brushes.Orange;
        public static SolidColorBrush Yellow => Brushes.Yellow;

        public static SolidColorBrush LightGreen => Brushes.LightGreen;
        public static SolidColorBrush OliveGreen { get; } 
            = CreateBrush(184, 215, 163);
        public static SolidColorBrush Teal { get; } 
            = CreateBrush(78, 201, 176);

        public static SolidColorBrush LightBlue => Brushes.SkyBlue;
        public static SolidColorBrush Blue { get; } 
            = CreateBrush(86, 156, 214);

        public static SolidColorBrush Purple { get; } 
            = CreateBrush(180, 155, 230);
    }
}