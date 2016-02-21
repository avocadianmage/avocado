using AvocadoServer.ServerAPI;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using UtilityLib.Processes;

namespace AvocadoServer.ServerCore
{
    class Host
    {
        public string TCPEndpoint => $"net.tcp://{internalIP}:{tcpPort}";
        public string MetadataEndpoint 
            => $"http://{internalIP}:{metadataPort}/ServerAPI";

        string internalIP => ConfigurationManager.AppSettings["InternalIP"];
        string tcpPort => ConfigurationManager.AppSettings["TCPPort"];
        string metadataPort => ConfigurationManager.AppSettings["metadataPort"];

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