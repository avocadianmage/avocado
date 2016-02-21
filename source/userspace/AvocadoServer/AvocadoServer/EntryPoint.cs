using AvocadoServer.Jobs;
using AvocadoServer.ServerCore;
using System;
using UtilityLib.Processes;

namespace AvocadoServer
{
    static class EntryPoint
    {
        public static JobList Jobs { get; } = new JobList();

        static void Main(string[] args)
        {
            // Parse commandline arguments.
            var metadata = EnvUtils.GetArg(0);
            var showMetadata = EnvUtils.GetArg(0)?.ToLower() == "metadata";

            // Start server.
            var host = new Host();
            host.Start(showMetadata);

            // Output log message that server has started.
            Console.Out.LogLine(
                $"AvocadoServer is now running ({Host.TCPEndpoint})");
            if (showMetadata)
            {
                Console.Out.LogLine(
                    $"Metadata is enabled for this session ({Host.MetadataEndpoint})");
            }

            // Restart existing jobs from disk.
            Console.Out.LogLine("Starting jobs...");
            Jobs.RestoreFromDisk();
            Console.Out.LogLine("Done restarting jobs.");
            
            // Block.
            Console.ReadKey();

            // Close server.
            host.Stop();
        }
    }
}
