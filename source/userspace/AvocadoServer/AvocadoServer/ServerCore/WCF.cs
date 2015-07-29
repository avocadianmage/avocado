using AvocadoServer.ServerAPI;
using AvocadoUtilities.CommandLine;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace AvocadoServer.ServerCore
{
    static class WCF
    {
        public static ServiceHost CreateHost()
        {
            var host = createHostBase();
            host.addEndpoint();
            host.enableMetadataExchange();
            host.showDebuggingInfo();
            EnvironmentMgr.RunCriticalCode(host.Open);
            return host;
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
                typeof(IServerAPI),
                new WSHttpBinding(),
                nameof(ServerAPIService));
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
    }
}