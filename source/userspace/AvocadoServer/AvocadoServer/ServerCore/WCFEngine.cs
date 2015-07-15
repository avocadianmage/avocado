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
            var host = createHostBase();
            host.addEndpoint();
            host.enableMetadataExchange();
            host.showDebuggingInfo();
            runCriticalCode(host.Open);
            return host;
        }

        public static ServerAPIClient CreateClient()
        {
            var client = new ServerAPIClient();
            runCriticalCode(() => client.Ping());
            return client;
        }

        static ServiceHost createHostBase()
        {
            return new ServiceHost(
                typeof(ServerAPIService),
                new Uri(ServerConfig.BaseAddress));
        }

        static void addEndpoint(this ServiceHost host)
        {
            host.AddServiceEndpoint(
                typeof(AvocadoServer.ServerAPI.IServerAPI),
                new WSHttpBinding(),
                ServerConfig.APIServiceName);
        }

        static void enableMetadataExchange(this ServiceHost host)
        {
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(smb);
        }

        static void showDebuggingInfo(this ServiceHost host)
        {
            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            var debug = new ServiceDebugBehavior
            {
                IncludeExceptionDetailInFaults = true
            };
            host.Description.Behaviors.Add(debug);
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