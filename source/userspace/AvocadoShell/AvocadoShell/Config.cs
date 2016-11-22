using System;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Brush PromptBrush => Brushes.LightGreen;
        public static Brush ElevatedBrush => Brushes.Orange;
        public static Brush ErrorFontBrush => Brushes.Salmon;
        public static Brush SystemFontBrush => Brushes.LightGray;

        public static ConsoleColor DefaultConsoleColor => ConsoleColor.Gray;
    }
}