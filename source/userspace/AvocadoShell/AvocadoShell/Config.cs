using System;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Brush PromptBrush => Brushes.LightGreen;
        public static Brush ElevatedBrush => Brushes.Yellow;
        public static Brush ErrorFontBrush => Brushes.Salmon;
        public static Brush SystemFontBrush => Brushes.LightGray;

        public static Brush InputBackgroundBrush { get; }
            = new SolidColorBrush(Colors.Gray) { Opacity = 0.25 };

        public static ConsoleColor SystemConsoleForeground 
            => ConsoleColor.Gray;
    }
}