using AvocadoServer.ServerAPI;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace AvocadoServer
{
    static class EntryPoint
    {
        static void Main(string[] args)
        {
            var host = createHost();

            try { host.Open(); }
            catch (AddressAccessDeniedException)
            {
                Console.Error.WriteLine(
                    "AvocadoServer must be run with elevated privileges.");
                Environment.Exit(0);
            }

            Console.WriteLine("Server is running...");
            Console.ReadLine();

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
