using AvocadoServer.Jobs;
using AvocadoServer.ServerCore;
using StandardLibrary.Processes;
using System;

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
            Logger.WriteLine(
                $"AvocadoServer is now running ({Host.TCPEndpoint})");
            if (showMetadata)
            {
                Logger.WriteLine(
                    $@"Metadata is enabled for this session ({
                        Host.MetadataEndpoint})");
            }

            // Restart existing jobs.
            Jobs.RestoreFromDisk();
            
            // Block.
            Console.ReadLine();

            // Close server.
            host.Stop();
        }
    }
}
