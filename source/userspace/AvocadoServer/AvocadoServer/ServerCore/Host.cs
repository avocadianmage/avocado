using AvocadoServer.ServerAPI;
using StandardLibrary.Processes;
using System.Linq;
using System.ServiceModel;

namespace AvocadoServer.ServerCore
{
    sealed class Host
    {
        readonly ServiceHost host = new ServiceHost(typeof(ServerAPIService));

        public void Start() => ConsoleProc.RunCriticalCode(host.Open);
        public void Stop() => host.Close();

        public string Endpoint => host.Description.Endpoints.Single().Address.ToString();
    }
}