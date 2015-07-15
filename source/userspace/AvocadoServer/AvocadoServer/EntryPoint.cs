using AvocadoServer.AvocadoServerService;
using AvocadoServer.ServerAPI;
using AvocadoServer.ServerCore;
using AvocadoUtilities.CommandLine;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using UtilityLib.MiscTools;

namespace AvocadoServer
{
    static class EntryPoint
    {
        static void Main(string[] args)
        {
            var error = Subcommand.Invoke();
            if (error != null) Console.Error.WriteLine(error);
        }

        [Subcommand]
        public static void Host(string[] args)
        {
            // The server cannot be hosted if the session is not elevated.
            if (!Tools.IsAdmin)
            {
                EnvironmentMgr.TerminatingError(
                    "AvocadoServer must be run with administrative privileges.");
            }

            var host = createHost();

            Logger.WriteServerStarted();
            Console.ReadKey();

            host.Close();
        }

        [Subcommand]
        public static void Ping(string[] args)
        {
            // Ping the server and measure the time taken.
            var stopwatch = Stopwatch.StartNew();
            createClient().Ping();
            stopwatch.Stop();
            var secs = stopwatch.ElapsedMilliseconds / 1000d;
            var timestamp = secs.ToRoundedString(3);

            // Output result.
            Console.WriteLine(
                "Reply in {0}s from AvocadoServer at {1}.",
                timestamp,
                ServerConfig.BaseAddress);
        }

        static ServiceHost createHost()
        {
            // Initialize ServiceHost.
            var host = new ServiceHost(
                typeof(ServerAPIService),
                new Uri(ServerConfig.BaseAddress));

            // Add endpoint.
            host.AddServiceEndpoint(
                typeof(AvocadoServer.ServerAPI.IServerAPI),
                new WSHttpBinding(),
                ServerConfig.APIServiceName);

            // Enable metadata exchange.
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(smb);

            // Start the host.
            runCriticalCode(host.Open);

            return host;
        }

        static ServerAPIClient createClient()
        {
            var client = new ServerAPIClient();
            runCriticalCode(() => client.Ping());
            return client;
        }

        static void runCriticalCode(Action action)
        {
            try { action.Invoke(); }
            catch (Exception e)
            {
                EnvironmentMgr.TerminatingError(e.Message);
            }
        }
    }
}
