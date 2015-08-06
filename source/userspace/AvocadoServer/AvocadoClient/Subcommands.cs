using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using UtilityLib.MiscTools;
using static AvocadoClient.WCF;
using static UtilityLib.Processes.ConsoleProc;

namespace AvocadoClient
{
    static class Subcommands
    {
        [Subcommand]
        public static void Ping(Arguments args)
        {
            // Ping the server and measure the time taken.
            var stopwatch = Stopwatch.StartNew();
            var client = CreateClient();
            stopwatch.Stop();
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            var addr = client.Endpoint.Address;
            Console.WriteLine($"Ping {addr} - completed in {timestamp}s.");
        }

        [Subcommand]
        public static void GetJobs(Arguments args)
        {
            var jobs = CreateClient().GetJobs();

            if (!jobs.Any())
            {
                Console.WriteLine("There are no jobs running.");
                return;
            }

            jobs.ForEach(Console.WriteLine);
        }

        [Subcommand]
        public static void RunJob(Arguments args)
        {
            var appName = args.PopNextArg();
            var progName = args.PopNextArg();
            var secInterval = args.PopNextArg<int>();

            if (appName == null || progName == null || secInterval == null)
            {
                TerminatingError(
                    "Expected: Server RunJob <app> <name> <interval> [args]");
            }

            var jobArgs = args.PopRemainingArgs().ToArray();
            
            // Call to server.
            var result = CreateClient().RunJob(
                appName, 
                progName, 
                secInterval.Value, 
                jobArgs);

            // Output result.
            result.LogToConsole();
        }

        [Subcommand]
        public static void KillJob(Arguments args)
        {
            var id = args.PopNextArg<int>();
            if (id == null)
            {
                TerminatingError("Expected: Server KillJob <id>");
            }

            // Call to server.
            var result = CreateClient().KillJob(id.Value);

            // Output result.
            result.LogToConsole();
        }
    }
}