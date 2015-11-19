using AvocadoServer.Jobs;
using AvocadoServer.ServerCore;
using System;
using System.Linq;

namespace AvocadoServer
{
    static class EntryPoint
    {
        public static JobList Jobs { get; } = new JobList();

        static void Main(string[] args)
        {
            // Start server.
            var host = WCF.CreateHost();

            // Output log message that server has started.
            var endpoint = host.BaseAddresses.First();
            Logger.WriteLine(
                $"AvocadoServer is now running at [{endpoint}]...");

            // Restart existing jobs from disk.
            Logger.WriteLine("Restarting jobs from last session...");
            Jobs.RestoreFromDisk();
            Logger.WriteLine("Done restarting jobs.");

            // Block.
            Console.ReadKey();

            // Close server.
            host.Close();
        }
    }
}
