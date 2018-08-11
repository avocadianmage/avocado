using AvocadoLib.CommandLine.ANSI;
using System.Collections.Generic;
using System.Windows.Media;

namespace AvocadoServiceHost.Utilities
{
    enum ColorType
    {
        None = 0,
        Timestamp = 1,
        KeyPhrase = 2,
        AlertLow = 3,
        AlertMed = 4,
        AlertHigh = 5
    }

    static class TextColorPalette
    {
        static readonly Dictionary<ColorType, Color> colorDict
            = new Dictionary<ColorType, Color>
            {
                { ColorType.Timestamp, Colors.DimGray },
                { ColorType.KeyPhrase, Color.FromRgb(86, 156, 214) },
                { ColorType.AlertLow, Colors.LightGreen },
                { ColorType.AlertMed, Colors.Yellow },
                { ColorType.AlertHigh, Colors.Orange}
            };

        public static string GetColorString(ColorType type, string text)
        {
            return type == ColorType.None
                ? text : ANSICode.GetColoredText(colorDict[type], text);
        }
    }
}
