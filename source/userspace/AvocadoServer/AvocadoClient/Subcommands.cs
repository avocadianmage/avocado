using AvocadoClient.ServerAPIReference;
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
            // Measure the time taken.
            var stopwatch = Stopwatch.StartNew();
            
            // Ping the server.
            var client = CreateClient();
            RunCommand(client.Ping);

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
            var result = RunCommand(CreateClient().GetJobs);

            var jobs = (result as PipelineOfArrayOfstringuHEDJ7Dj).Data;
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
                = "{0} Expected: Client {1} <filename> [interval]";

            // Get required filename parameter.
            var filename = args.PopArg();
            if (filename == null)
            {
                TerminatingError(string.Format(
                    ERROR_FORMAT, 
                    "Missing <filename> parameter.", 
                    nameof(RunJob)));
            }

            // Get optional interval parameter.
            var secInterval = args.PopArg<int>();

            // Call to server.
            RunCommand(() => CreateClient().RunJob(filename, secInterval));
        }

        [Subcommand]
        public static void KillJob(Arguments args)
        {
            var id = args.PopArg<int>();
            if (id == null)
            {
                TerminatingError("Expected: Client KillJob <id>");
            }

            // Call to server.
            RunCommand(() => CreateClient().KillJob(id.Value));
        }
    }
}