using System;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Brush PromptBrush { get { return Brushes.LightGreen; } }
        public static Brush ErrorFontBrush { get { return Brushes.Salmon; } }

        public static Brush SystemFontBrush 
        { 
            get { return Brushes.LightGray; } 
        }

        public static ConsoleColor DefaultConsoleColor
        {
            get { return ConsoleColor.Gray; }
        }
    }
}