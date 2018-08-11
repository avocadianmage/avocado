using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoLib.CommandLine.ANSI
{
    public static class ANSIWriter
    {
        public static void WriteLine(string text)
        {
            WriteLine(new(string, ColorType)[] { (text, ColorType.None) });
        }

        public static void WriteLine(
            IEnumerable<(string text, ColorType type)> segments)
        {
            Console.WriteLine(segments
                .Select(s => ColorPalette.GetColorString(s.type, s.text))
                .Aggregate((a, n) => a += n));
        }
    }
}
