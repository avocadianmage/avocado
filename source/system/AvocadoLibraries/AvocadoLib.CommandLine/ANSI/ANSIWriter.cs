using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AvocadoLib.CommandLine.ANSI
{
    public enum ColorType
    {
        None = 0,
        Informational = 1,
        KeyPhrase = 2,
        AlertLow = 3,
        AlertMed = 4,
        AlertHigh = 5
    }

    public static class ANSIWriter
    {
        static readonly Dictionary<ColorType, Color> colorDict
            = new Dictionary<ColorType, Color>
            {
                { ColorType.Informational, Colors.DimGray },
                { ColorType.KeyPhrase, Color.FromRgb(86, 156, 214) },
                { ColorType.AlertLow, Colors.LightGreen },
                { ColorType.AlertMed, Colors.Yellow },
                { ColorType.AlertHigh, Colors.Orange}
            };

        static string getColorString(ColorType type, string text)
        {
            return type == ColorType.None
                ? text : ANSICode.GetColoredText(colorDict[type], text);
        }

        public static void WriteLine(
            IEnumerable<(string text, ColorType type)> segments)
        {
            Console.WriteLine(segments
                .Select(s => getColorString(s.type, s.text))
                .Aggregate((a, n) => a += n));
        }
    }
}
