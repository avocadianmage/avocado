using AvocadoClient.AvocadoServerService;
using UtilityLib.Processes;

namespace AvocadoClient
{
    static class WCF
    {
        public static ServerAPIClient CreateClient()
        {
            var client = new ServerAPIClient();
            ConsoleProc.RunCriticalCode(() => client.Ping());
            return client;
        }
    }
}