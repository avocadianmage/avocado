using AvocadoServer.AvocadoServerService;
using AvocadoServer.ServerAPI;
using AvocadoUtilities.CommandLine;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace AvocadoServer.ServerCore
{
    static class WCFEngine
    {
        public static ServiceHost CreateHost()
        {
            // Initialize ServiceHost.
            var host = new ServiceHost(
                typeof(ServerAPIService),
                new Uri(ServerConfig.BaseAddress));

            // Add endpoint.
            host.AddServiceEndpoint(
                typeof(AvocadoServer.ServerAPI.IServerAPI),
                new WSHttpBinding(),
                ServerConfig.APIServiceName);

            // Enable metadata exchange.
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(smb);

            // Start the host.
            runCriticalCode(host.Open);

            return host;
        }

        public static ServerAPIClient CreateClient()
        {
            var client = new ServerAPIClient();
            runCriticalCode(() => client.Ping());
            return client;
        }

        static void runCriticalCode(Action action)
        {
            try { action.Invoke(); }
            catch (Exception e)
            {
                EnvironmentMgr.TerminatingError(e.Message);
            }
        }
    }
}