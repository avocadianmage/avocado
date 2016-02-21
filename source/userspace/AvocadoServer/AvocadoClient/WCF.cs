using AvocadoClient.ServerAPIReference;
using System.Security.Principal;
using UtilityLib.Processes;

namespace AvocadoClient
{
    static class WCF
    {
        public static ServerAPIClient CreateClient()
        {
            var client = new ServerAPIClient();
            client.ClientCredentials.Windows.AllowedImpersonationLevel
                = TokenImpersonationLevel.Impersonation;
            ConsoleProc.RunCriticalCode(() => client.Ping());
            return client;
        }
    }
}