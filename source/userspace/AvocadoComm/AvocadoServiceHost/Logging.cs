using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using AvocadoLib.CommandLine.ANSI;

namespace AvocadoServiceHost
{
    static class Logging
    {
        public static void WriteLine(string text)
        {
            WriteLine(new(string, Color?)[] { (text, null) });
        }

        public static void WriteLine(
            IEnumerable<(string text, Color? color)> segments)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.f ");
            Console.WriteLine(segments
                .Prepend((timestamp, Colors.DimGray))
                .Select(s =>
                {
                    return s.color.HasValue
                        ? ANSICode.GetColoredText(s.color.Value, s.text)
                        : s.text;
                })
                .Aggregate((a, n) => a += n));
        }
    }
}