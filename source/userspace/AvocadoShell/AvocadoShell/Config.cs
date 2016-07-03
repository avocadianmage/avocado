using System;
using System.Reflection;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Version Version
            => Assembly.GetExecutingAssembly().GetName().Version;

        public static Brush PromptBrush => Brushes.LightGreen;
        public static Brush ErrorFontBrush => Brushes.Salmon;
        public static Brush SystemFontBrush => Brushes.LightGray;

        public static ConsoleColor DefaultConsoleColor => ConsoleColor.Gray;

        public static int MaxBufferWidth => 10000;
    }
}