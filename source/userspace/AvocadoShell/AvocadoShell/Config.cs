using AvocadoFramework.Controls.TextRendering;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Windows.Media;

namespace AvocadoShell
{
    sealed class Config
    {
        public static Brush PromptBrush => TextPalette.LightGreen;
        public static Brush ElevatedBrush => TextPalette.Yellow;
        public static Brush ErrorBrush => TextPalette.LightRed;
        public static Brush OutputBrush => TextPalette.LightGray;
        public static Brush ProgressBrush => TextPalette.LightBlue;
        public static Brush DebugBrush => TextPalette.LightBlue;
        public static Brush VerboseBrush => TextPalette.DarkGray;
        public static Brush WarningBrush => TextPalette.Orange;
        public static Brush SelectedBrush => TextPalette.Orange;

        public static ConsoleColor SystemConsoleForeground => ConsoleColor.Gray;

        public static Dictionary<PSTokenType, Brush> PSSyntaxColorLookup
            { get; } = new Dictionary<PSTokenType, Brush>
            {
                { PSTokenType.Attribute, TextPalette.Teal },
                { PSTokenType.Command, TextPalette.LightBlue },
                { PSTokenType.CommandArgument, null },
                { PSTokenType.CommandParameter, TextPalette.Purple },
                { PSTokenType.Comment, TextPalette.DarkGray},
                { PSTokenType.GroupStart, null },
                { PSTokenType.GroupEnd, null },
                { PSTokenType.Keyword, TextPalette.Blue },
                { PSTokenType.LineContinuation, TextPalette.Orange },
                { PSTokenType.Member, null },
                { PSTokenType.NewLine, null },
                { PSTokenType.Number, TextPalette.OliveGreen },
                { PSTokenType.Operator, null },
                { PSTokenType.StatementSeparator, TextPalette.Orange },
                { PSTokenType.String, TextPalette.Yellow },
                { PSTokenType.Type, TextPalette.Teal },
                { PSTokenType.Variable, TextPalette.OliveGreen },
                { PSTokenType.Unknown, null }
            };
    }
}