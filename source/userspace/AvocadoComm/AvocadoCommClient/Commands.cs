using AvocadoLib.CommandLine.ANSI;
using AvocadoLib.CommandLine.Arguments;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace AvocadoCommClient
{
    static class Commands
    {
        [Subcommand]
        public static void Ping(IEnumerable<string> args)
        {
            // Measure the time taken.
            var stopwatch = Stopwatch.StartNew();

            // Ping the server.
            var result = ClientFactory.ExecuteClientCommand(
                c => c.Ping(), out EndpointAddress endpoint);

            stopwatch.Stop();

            // Output result.
            var msElapsed = stopwatch.ElapsedMilliseconds;
            ANSIWriter.WriteLine(new[]
            {
                ("Ping to ", ColorType.None),
                (endpoint.ToString(), ColorType.KeyPhrase),
                (" returned ", ColorType.None),
                (result.ToString(), ColorType.KeyPhrase),
                (" in ", ColorType.None),
                (msElapsed.ToString(), getLatencyColor(msElapsed)),
                (" ms.", ColorType.None)
            });
        }

        static ColorType getLatencyColor(long msElapsed)
        {
            if (msElapsed < 100) return ColorType.AlertLow;
            if (msElapsed < 300) return ColorType.AlertMed;
            return ColorType.AlertHigh;
        }
    }
}
