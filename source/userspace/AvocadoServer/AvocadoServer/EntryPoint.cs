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
            var host = WCF.CreateHost();

            var endpointStr = host.BaseAddresses.First().ToString();
            Logger.WriteServerStarted(endpointStr);
            Console.ReadKey();

            host.Close();
        }
    }
}
