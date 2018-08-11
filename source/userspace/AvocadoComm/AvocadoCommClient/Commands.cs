using AvocadoLib.Basic;
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
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            ANSIWriter.WriteLine(new[]
            {
                ("Ping to ", ColorType.None),
                (endpoint.ToString(), ColorType.KeyPhrase),
                (" returned ", ColorType.None),
                (result.ToString(), ColorType.KeyPhrase),
                (" in ", ColorType.None),
                (timestamp, getLatencyColor(secs)),
                (".", ColorType.None)
            });
        }

        static ColorType getLatencyColor(double secondsElapsed)
        {
            if (secondsElapsed < 0.1) return ColorType.AlertLow;
            if (secondsElapsed < 0.3) return ColorType.AlertMed;
            return ColorType.AlertHigh;
        }
    }
}
