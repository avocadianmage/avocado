using AvocadoServer.ServerAPI;
using System;
using System.Reflection;

namespace AvocadoServer.ServerCore
{
    static class CommandContext
    {
        public static Pipeline ExecuteRequest(
            Action<Pipeline> action, MethodBase method, params object[] args)
        {
            var result = ExecuteRequest(
                p => { action(p); return default(object); },
                method,
                args);
            return new Pipeline
            {
                Success = result.Success,
                Message = result.Message
            };
        }

        public static Pipeline<T> ExecuteRequest<T>(
            Func<Pipeline, T> func, MethodBase method, params object[] args)
        {
            var pipeline = new Pipeline<T>();
            runChecks(pipeline, method, args);

            try
            {
                if (!pipeline.Success) return pipeline;

                pipeline.Data = func(pipeline);
                return pipeline;
            }
            finally { pipeline.Log(); }
        }

        static void runChecks(
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
            return attr.Client >= ClientIdentifier.GetClientType();
        }
    }
}