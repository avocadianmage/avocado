using StandardLibrary.Processes;
using System;
using System.Reflection;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Version Version { get; }
            = Assembly.GetExecutingAssembly().GetName().Version;

        public static Brush PromptBrush { get; } 
            = EnvUtils.IsAdmin ? Brushes.Yellow : Brushes.LightGreen;
        public static Brush ErrorFontBrush => Brushes.Salmon;
        public static Brush SystemFontBrush => Brushes.LightGray;
        public static Brush InputBrush => Brushes.SkyBlue;

        public static ConsoleColor DefaultConsoleColor => ConsoleColor.Gray;
    }
}