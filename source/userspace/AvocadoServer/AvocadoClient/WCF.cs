using AvocadoClient.ServerAPIReference;
using System.Security.Principal;

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
    }
}