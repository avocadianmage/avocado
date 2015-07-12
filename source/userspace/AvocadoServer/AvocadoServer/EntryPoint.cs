using AvocadoServer.ServerAPI;
using AvocadoServer.ServerCore;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using UtilityLib.MiscTools;

namespace AvocadoServer
{
    static class EntryPoint
    {
        static void Main(string[] args)
        {
            // The server must be run as an administrator.
            if (!Tools.IsAdmin)
            {
                Console.Error.WriteLine(
                    "AvocadoServer must be run with administrative privileges.");
                Environment.Exit(0);
            }

            var host = createHost();

            host.Open();

            Logger.WriteServerStarted();
            Console.ReadKey();

            host.Close();
        }

        static ServiceHost createHost()
        {
            // Initialize ServiceHost.
            var host = new ServiceHost(
                typeof(ServerAPIService),
                new Uri(ServerConfig.BaseAddress));

            // Add endpoint.
            host.AddServiceEndpoint(
                typeof(IServerAPI),
                new WSHttpBinding(),
                ServerConfig.APIServiceName);

            // Enable metadata exchange.
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(smb);

            return host;
        }
    }
}
