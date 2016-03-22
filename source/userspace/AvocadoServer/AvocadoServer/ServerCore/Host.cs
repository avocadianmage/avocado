using AvocadoServer.ServerAPI;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using UtilityLib.Processes;

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
            addTCPEndpoint(host);
            if (withMetadata) addMetadataEndpoint(host);
            ConsoleProc.RunCriticalCode(host.Open);
        }

        public void Stop() => host.Close();

        void addTCPEndpoint(ServiceHost host)
        {
            host.AddServiceEndpoint(
                typeof(IServerAPI),
                new NetTcpBinding(),
                TCPEndpoint);
        }

        void addMetadataEndpoint(ServiceHost host)
        {
            host.Description.Behaviors.Add(new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                HttpGetUrl = new Uri(MetadataEndpoint)
            });
        }
    }
}