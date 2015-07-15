using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using UtilityLib.MiscTools;

namespace AvocadoServer.ServerCore
{
    static class Subcommands
    {
        [Subcommand]
        public static void Host(string[] args)
        {
            // The server cannot be hosted if the session is not elevated.
            if (!Tools.IsAdmin)
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
            WCFEngine.CreateClient().RunJob("test", "job", null);
        }
    }
}