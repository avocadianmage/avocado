using System;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static string Version { get; } = "0.8.2";

        public static Brush PromptBrush { get; } = Brushes.LightGreen;
        public static Brush ErrorFontBrush { get; } = Brushes.Salmon;
        public static Brush SystemFontBrush { get; } = Brushes.LightGray;

        public static ConsoleColor DefaultConsoleColor { get; } 
            = ConsoleColor.Gray;
    }
}