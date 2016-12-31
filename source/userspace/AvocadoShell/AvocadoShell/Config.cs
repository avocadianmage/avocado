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
            = createBrush(Colors.Gray, 0.25);

        public static ConsoleColor SystemConsoleForeground 
            => ConsoleColor.Gray;

        public static Dictionary<PSTokenType, Brush> PSSyntaxColorLookup
            = new Dictionary<PSTokenType, Brush>
            {
                { PSTokenType.Attribute, createBrush(78, 201, 176) },
                { PSTokenType.Command, Brushes.SkyBlue },
                { PSTokenType.CommandArgument, null },
                { PSTokenType.CommandParameter, createBrush(180, 155, 230) },
                { PSTokenType.Comment, Brushes.DimGray },
                { PSTokenType.GroupStart, null },
                { PSTokenType.GroupEnd, null },
                { PSTokenType.Keyword, createBrush(86, 156, 214) },
                { PSTokenType.LineContinuation, Brushes.Orange },
                { PSTokenType.Member, null },
                { PSTokenType.NewLine, null },
                { PSTokenType.Number, createBrush(184, 215, 163) },
                { PSTokenType.Operator, null },
                { PSTokenType.StatementSeparator, Brushes.Orange },
                { PSTokenType.String, Brushes.Yellow },
                { PSTokenType.Type, createBrush(78, 201, 176) },
                { PSTokenType.Variable, createBrush(184, 215, 163) },
                { PSTokenType.Unknown, null }
            };

        static Brush createBrush(byte r, byte g, byte b, double opacity = 1)
            => createBrush(Color.FromRgb(r, g, b), opacity);

        static Brush createBrush(Color color, double opacity = 1)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();
            return brush;
        }
    }
}