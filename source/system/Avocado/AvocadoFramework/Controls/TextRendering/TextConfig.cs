using System.Windows;
using System.Windows.Media;

namespace AvocadoFramework.Controls.TextRendering
{
    static class TextConfig
    {
        public static FontFamily FontFamily { get; } 
            = new FontFamily("Consolas");
        public static double FontSize => 9.5;
        public static FontWeight FontWeight => FontWeights.Bold;
        public static TextFormattingMode TextFormattingMode 
            => TextFormattingMode.Display;
    }
}
