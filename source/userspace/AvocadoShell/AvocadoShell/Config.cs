using AvocadoFramework.Controls.TextRendering;
using System;
using System.Management.Automation.Language;
using System.Windows.Media;
using System.Windows.Threading;

namespace AvocadoShell
{
    sealed class Config
    {
        public static DispatcherPriority TextPriority 
            => DispatcherPriority.ContextIdle;

        public static ConsoleColor SystemConsoleForeground => ConsoleColor.Gray;

        public static Brush PromptBrush => TextPalette.LightGreen;
        public static Brush ElevatedBrush => TextPalette.Orange;
        public static Brush ErrorBrush => TextPalette.LightRed;
        public static Brush OutputBrush => TextPalette.LightGray;
        public static Brush ProgressBrush => TextPalette.LightBlue;
        public static Brush DebugBrush => TextPalette.LightBlue;
        public static Brush VerboseBrush => TextPalette.DarkGray;
        public static Brush WarningBrush => TextPalette.Yellow;
        public static Brush SelectedBrush => TextPalette.Orange;

        public static Brush GetTokenBrush(Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Comment: return TextPalette.DarkGray;
                case TokenKind.Number: return TextPalette.OliveGreen;
                case TokenKind.Parameter: return TextPalette.Purple;
                case TokenKind.StringExpandable: return TextPalette.Yellow;
                case TokenKind.Variable: return TextPalette.OliveGreen;
            }

            if (token.TokenFlags.HasFlag(TokenFlags.CommandName))
            {
                return TextPalette.LightBlue;
            }
            if (token.TokenFlags.HasFlag(TokenFlags.Keyword))
            {
                return TextPalette.Blue;
            }
            if (token.TokenFlags.HasFlag(TokenFlags.TypeName))
            {
                return TextPalette.Teal;
            }

            return null;
        }
    }
}