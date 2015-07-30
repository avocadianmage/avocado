using AvocadoServer.ServerAPI;
using System.ServiceModel;
using UtilityLib.Processes;

namespace AvocadoServer.ServerCore
{
    static class WCF
    {
        public static ServiceHost CreateHost()
        {
            var host = new ServiceHost(typeof(ServerAPIService));
            ConsoleProc.RunCriticalCode(host.Open);
            return host;
        }
    }
}