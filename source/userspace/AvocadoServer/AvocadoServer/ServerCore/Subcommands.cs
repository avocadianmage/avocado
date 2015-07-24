using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoServer.ServerCore
{
    static class Subcommands
    {
        [Subcommand]
        public static void Host(string[] args)
        {
            // The server cannot be hosted if the session is not elevated.
            if (!EnvUtils.IsAdmin)
            {
                EnvironmentMgr.TerminatingError(
                    "AvocadoServer must be run with administrative privileges.");
            }

            var host = WCFEngine.CreateHost();

            Logger.WriteServerStarted();
            Console.ReadKey();

            host.Close();
        }

        [Subcommand]
        public static void Ping(string[] args)
        {
            // Ping the server and measure the time taken.
            var stopwatch = Stopwatch.StartNew();
            WCFEngine.CreateClient();
            stopwatch.Stop();
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            const string FMT = "Ping {0} - completed in {1}s.";
            Console.WriteLine(FMT, ServerConfig.BaseAddress, timestamp);
        }

        [Subcommand]
        public static void RunJob(string[] args)
        {
            // Retrieve and verify the correct number of arguments are given.
            var appName = args.ElementAtOrDefault(0);
            var progName = args.ElementAtOrDefault(1);
            if (appName == null || progName == null)
            {
                EnvironmentMgr.TerminatingError(
                    "Expected: Server RunJob <app> <name>");
            }

            var jobArgs = args.Skip(2).ToArray();
            WCFEngine.CreateClient().RunJob(appName, progName, jobArgs);
        }
    }
}