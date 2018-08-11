using AvocadoLib.Basic;
using AvocadoLib.CommandLine.Arguments;
using System;
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
            Console.WriteLine(
                $"Ping to {endpoint} returned '{result}' in {timestamp}s.");
        }

        
    }
}
