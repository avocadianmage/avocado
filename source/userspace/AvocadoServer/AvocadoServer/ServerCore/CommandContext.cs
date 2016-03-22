using AvocadoServer.ServerAPI;
using System.Reflection;

namespace AvocadoServer.ServerCore
{
    static class CommandContext
    {
        public static void RunChecks(
            Pipeline pipeline, MethodBase method, params object[] args)
        {
            // Log the action the client is requesting.
            Logger.WriteLine(method, args);

            // Verify the client has sufficient access to run the command.
            pipeline.Success = performClientCheck(method);
            if (!pipeline.Success)
            {
                pipeline.Message
                    = $"Client does not have sufficient access to execute {method.Name}.";
            }
        }

        static bool performClientCheck(MethodBase method)
        {
            var attr = method.GetCustomAttribute<AllowedClientAttribute>();
            var allowedClient = attr?.Client ?? ClientType.ThisMachine;
            return allowedClient >= ClientIdentifier.GetClientType();
        }
    }
}