using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.Processes;
using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace AvocadoShell
{
    sealed class Config
    {
        public static DispatcherPriority TextPriority 
            => DispatcherPriority.ContextIdle;

        public static ConsoleColor SystemConsoleForeground => ConsoleColor.Gray;

        public static Brush ErrorBrush => TextPalette.LightRed;
        public static Brush OutputBrush => TextPalette.LightGray;
        public static Brush ProgressBrush => TextPalette.LightBlue;
        public static Brush DebugBrush => TextPalette.LightBlue;
        public static Brush VerboseBrush => TextPalette.DarkGray;
        public static Brush WarningBrush => TextPalette.Yellow;
        public static Brush SelectedBrush => TextPalette.Yellow;
        public static Brush PromptBrush { get; } 
            = EnvUtils.IsAdmin ? TextPalette.Orange : TextPalette.LightGreen;
    }
}