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
            var addr = client.Endpoint.Address;
            Console.WriteLine($"Ping {addr} - completed in {timestamp}s.");
        }

        [Subcommand]
        public static void GetJobs(string[] args)
        {
            WCF.CreateClient().GetJobs().ForEach(Console.WriteLine);
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
            stream.WriteLine(result.Message); //TODO: generalize this
        }

        [Subcommand]
        public static void KillJob(string[] args)
        {
            // Retrieve and validate arguments.
            var idStr = args.ElementAtOrDefault(0);
            if (idStr == null)
            {
                ConsoleProc.TerminatingError(
                    "Expected: Server KillJob <id>");
            }

            // Convert id to an integer.
            int id;
            if (!int.TryParse(idStr, out id))
            {
                ConsoleProc.TerminatingError(
                    "Argument <id> must be an integer.");
            }

            // Call to server.
            var client = WCF.CreateClient();
            var result = client.KillJob(id);

            // Output result.
            var stream = result.Success ? Console.Out : Console.Error;
            stream.WriteLine(result.Message); //TODO: generalize this
        }
    }
}