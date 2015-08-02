using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoClient
{
    static class Subcommands
    {
        [Subcommand]
        public static void Ping(string[] args)
        {
            // Ping the server and measure the time taken.
            var stopwatch = Stopwatch.StartNew();
            var client = WCF.CreateClient();
            stopwatch.Stop();
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            const string FMT = "Ping {0} - completed in {1}s.";
            Console.WriteLine(FMT, client.Endpoint.Address, timestamp);
        }

        [Subcommand]
        public static void RunJob(string[] args)
        {
            var i = 0;

            // Retrieve and verify the correct number of arguments are given.
            var appName = args.ElementAtOrDefault(i++);
            var progName = args.ElementAtOrDefault(i++);
            var secIntervalStr = args.ElementAtOrDefault(i++);
            if (appName == null || progName == null || secIntervalStr == null)
            {
                ConsoleProc.TerminatingError(
                    "Expected: Server RunJob <app> <name> <interval> [args]");
            }

            var jobArgs = args.Skip(i).ToArray();

            // Convert secInterval to an integer.
            int secInterval;
            if (!int.TryParse(secIntervalStr, out secInterval))
            {
                ConsoleProc.TerminatingError(
                    "Argument <interval> must be an integer.");
            }

            // Call to server.
            var client = WCF.CreateClient();
            var result = client.RunJob(appName, progName, secInterval, jobArgs);

            // Output result.
            var stream = result.Success ? Console.Out : Console.Error;
            stream.WriteLine(result.Message);
        }
    }
}