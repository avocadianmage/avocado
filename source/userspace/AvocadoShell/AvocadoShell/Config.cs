using System;
using System.Collections.Generic;
using System.Management.Automation;
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
            = new SolidColorBrush(Colors.Gray) { Opacity = 0.25, };

        public static ConsoleColor SystemConsoleForeground 
            => ConsoleColor.Gray;

        public static Dictionary<PSTokenType, Color?> PSSyntaxColorLookup
            = new Dictionary<PSTokenType, Color?>
            {
                { PSTokenType.Attribute, Color.FromRgb(78, 201, 176) },
                { PSTokenType.Command, Colors.SkyBlue },
                { PSTokenType.CommandArgument, null },
                { PSTokenType.CommandParameter, Color.FromRgb(180, 155, 230) },
                { PSTokenType.Comment, Colors.DimGray },
                { PSTokenType.GroupStart, null },
                { PSTokenType.GroupEnd, null },
                { PSTokenType.Keyword, Color.FromRgb(86, 156, 214) },
                { PSTokenType.LineContinuation, Colors.Orange },
                { PSTokenType.Member, null },
                { PSTokenType.NewLine, null },
                { PSTokenType.Number, Color.FromRgb(184, 215, 163) },
                { PSTokenType.Operator, null },
                { PSTokenType.StatementSeparator, Colors.Orange },
                { PSTokenType.String, Colors.Yellow },
                { PSTokenType.Type, Color.FromRgb(78, 201, 176) },
                { PSTokenType.Variable, Color.FromRgb(184, 215, 163) },
                { PSTokenType.Unknown, null }
            };

        static Config()
        {
            InputBackgroundBrush.Freeze();
        }
    }
}