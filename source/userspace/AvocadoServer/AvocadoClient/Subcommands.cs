using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using System.IO;
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
            Console.WriteLine($"Ping to {addr} completed in {timestamp}s.");
        }

        [Subcommand]
        public static void GetJobs(Arguments args)
        {
            var result = CreateClient().GetJobs();
            result.Log();

            var jobs = result.Data;
            if (jobs == null) return;

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
            const string ERROR_FORMAT 
                = "{0} Expected: Client {1} <filename> <interval> [args]";

            // Get required filename parameter.
            var filename = args.PopNextArg();
            if (filename == null)
            {
                TerminatingError(string.Format(
                    ERROR_FORMAT, 
                    "Missing <filename> parameter.", 
                    nameof(RunJob)));
            }

            // Get required interval parameter.
            var secInterval = args.PopNextArg<int>();
            if (secInterval == null)
            {
                TerminatingError(string.Format(
                    ERROR_FORMAT,
                    "Missing <interval> parameter.",
                    nameof(RunJob)));
            }
            
            // Get any optional job arguments.
            var jobArgs = args.PopRemainingArgs().ToArray();
            
            // Call to server.
            var result = CreateClient().RunJob(
                filename, 
                secInterval.Value, 
                jobArgs);

            // Output result.
            result.Log();
        }

        [Subcommand]
        public static void KillJob(Arguments args)
        {
            var id = args.PopNextArg<int>();
            if (id == null)
            {
                TerminatingError("Expected: Client KillJob <id>");
            }

            // Call to server.
            var result = CreateClient().KillJob(id.Value);

            // Output result.
            result.Log();
        }
    }
}