using AvocadoServer.ServerAPI;
using StandardLibrary.Processes;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace AvocadoServer.ServerCore
{
    sealed class Host
    {
        public static string InternalIP 
            => ConfigurationManager.AppSettings["InternalIP"];
        public static string TcpPort 
            => ConfigurationManager.AppSettings["TCPPort"];
        public static string MetadataPort 
            => ConfigurationManager.AppSettings["MetadataPort"];

        public static string TCPEndpoint => $"net.tcp://{InternalIP}:{TcpPort}";
        public static string MetadataEndpoint
            => $"http://{InternalIP}:{MetadataPort}/ServerAPI";

        readonly ServiceHost host = new ServiceHost(typeof(ServerAPIService));

        public void Start(bool withMetadata)
        {
            addTCPEndpoint();
            addBehaviors(withMetadata);
            ConsoleProc.RunCriticalCode(host.Open);
        }

        public void Stop() => host.Close();

        void addTCPEndpoint()
        {
            host.AddServiceEndpoint(
                typeof(IServerAPI), new NetTcpBinding(), TCPEndpoint);
        }

        void addBehaviors(bool withMetadata)
        {
            var behaviors = host.Description.Behaviors;

            // Add metadata end point.
            if (withMetadata)
            {
                behaviors.Add(new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = new Uri(MetadataEndpoint)
                });
            }

            // Add debugging information.
            var debugBehaviorType = typeof(ServiceDebugBehavior);
            if (!behaviors.Contains(debugBehaviorType))
            {
                behaviors.Add(new ServiceDebugBehavior());
            }
            (behaviors[debugBehaviorType] as ServiceDebugBehavior)
                .IncludeExceptionDetailInFaults = true;
        }
    }
}