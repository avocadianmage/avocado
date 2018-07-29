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

            // Start server.
            var host = new Host();
            host.Start();

            // Output log message that server has started.
            Logger.WriteLine($"AvocadoServer is now running ({host.Endpoint})");

            // Restart existing jobs.
            Jobs.RestoreFromDisk();
            
            // Block.
            Console.ReadLine();

            // Close server.
            host.Stop();
        }
    }
}
