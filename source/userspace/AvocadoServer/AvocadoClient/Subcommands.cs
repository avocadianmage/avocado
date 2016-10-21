using AvocadoClient.ServerAPIReference;
using AvocadoUtilities.CommandLine;
using StandardLibrary.Extensions;
using StandardLibrary.Processes;
using System;
using System.Diagnostics;
using System.IO;
using static AvocadoClient.WCF;

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

            var jobs = (result as PipelineOfstring).Data;
            if (string.IsNullOrWhiteSpace(jobs))
            {
                Console.WriteLine("There are no jobs running.");
                return;
            }

            Console.WriteLine(jobs);
        }

        [Subcommand]
        public static void KillJob(Arguments args)
        {
            var id = args.PopArg<int>();
            if (id == null)
            {
                TerminatingError("Expected: Client KillJob <id>");
            }

            // Execute command on server.
            RunCommand(() => CreateClient().KillJob(id.Value));
        }

        private static void TerminatingError(string v)
        {
            throw new NotImplementedException();
        }

        [Subcommand]
        public static void RunJob(Arguments args)
        {
            const string ERROR_FORMAT
                = "{0} Expected: Client {1} <interval> <filename>";

            // Get optional interval parameter.
            var secInterval = args.PopArg<int>();
            if (secInterval == null)
            {
                TerminatingError(string.Format(
                    ERROR_FORMAT,
                    "Missing <interval> parameter.",
                    nameof(RunJob)));
            }

            // Get required filename parameter.
            var filename = string.Join(" ", args.PopRemainingArgs());
            if (string.IsNullOrWhiteSpace(filename))
            {
                TerminatingError(string.Format(
                    ERROR_FORMAT,
                    "Missing <filename> parameter.",
                    nameof(RunJob)));
            }

            // Execute command on server.
            RunCommand(() => CreateClient().RunJob(
                Directory.GetCurrentDirectory(), secInterval.Value, filename));
        }

        [Subcommand]
        public static void Run(Arguments args)
        {
            // Get required filename parameter.
            var filename = string.Join(" ", args.PopRemainingArgs());
            if (string.IsNullOrWhiteSpace(filename))
            {
                TerminatingError($"Expected: Client {nameof(Run)} <filename>");
            }

            // Execute command on server.
            RunCommand(() => CreateClient().RunJob(
                Directory.GetCurrentDirectory(), 0, filename));
        }
    }
}