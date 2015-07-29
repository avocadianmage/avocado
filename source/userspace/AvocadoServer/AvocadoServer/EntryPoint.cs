using AvocadoServer.ServerCore;
using System;
using System.Linq;

namespace AvocadoServer
{
    static class EntryPoint
    {
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
