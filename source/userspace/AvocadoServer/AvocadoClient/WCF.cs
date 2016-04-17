using AvocadoClient.ServerAPIReference;
using System;
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
            return client;
        }

        public static Pipeline RunCommand(Func<Pipeline> func)
        {
            var result = ConsoleProc.RunCriticalCode(func);
            result.Log();
            return result;
        }
    }
}