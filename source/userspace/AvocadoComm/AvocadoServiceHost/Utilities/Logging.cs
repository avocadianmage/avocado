using AvocadoLib.CommandLine.ANSI;
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
            ANSIWriter.WriteLine(
                segments.Prepend((timestamp, ColorType.Informational)));
        }
    }
}
