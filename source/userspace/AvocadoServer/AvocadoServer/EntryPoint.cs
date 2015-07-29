using AvocadoServer.ServerCore;
using AvocadoUtilities.CommandLine;
using System;
using UtilityLib.Processes;

namespace AvocadoServer
{
    static class EntryPoint
    {
        static void Main(string[] args)
        {
            // The server cannot be hosted if the session is not elevated.
            if (!EnvUtils.IsAdmin)
            {
                EnvironmentMgr.TerminatingError(
                    "AvocadoServer must be run with administrative privileges.");
            }

            var host = WCF.CreateHost();

            Logger.WriteServerStarted();
            Console.ReadKey();

            host.Close();
        }
    }
}
