using AvocadoServer.ServerAPI;
using AvocadoUtilities.CommandLine;
using System.ServiceModel;

namespace AvocadoServer.ServerCore
{
    static class WCF
    {
        public static ServiceHost CreateHost()
        {
            var host = new ServiceHost(typeof(ServerAPIService));
            EnvironmentMgr.RunCriticalCode(host.Open);
            return host;
        }
    }
}