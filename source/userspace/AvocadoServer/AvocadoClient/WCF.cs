using AvocadoClient.AvocadoServerService;
using AvocadoUtilities.CommandLine;

namespace AvocadoClient
{
    static class WCF
    {
        public static ServerAPIClient CreateClient()
        {
            var client = new ServerAPIClient();
            EnvironmentMgr.RunCriticalCode(() => client.Ping());
            return client;
        }
    }
}