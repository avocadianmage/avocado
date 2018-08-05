using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoServiceHost.Utilities
{
    static class Logging
    {
        public static void WriteLine(string text)
        {
            WriteLine(new(string, ColorType)[] { (text, ColorType.None) });
        }

        public static void WriteLine(
            IEnumerable<(string text, ColorType type)> segments)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.f ");
            Console.WriteLine(segments
                .Prepend((text: timestamp, type: ColorType.Timestamp))
                .Select(s => TextColorPalette.GetColorString(s.type, s.text))
                .Aggregate((a, n) => a += n));
        }
    }
}
