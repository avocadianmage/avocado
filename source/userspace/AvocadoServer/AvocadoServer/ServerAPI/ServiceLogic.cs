using AvocadoServer.ServerCore;
using System;
using System.Reflection;

namespace AvocadoServer.ServerAPI
{
    static class ServiceLogic
    {
        public static Pipeline ExecuteRequest(
            Action<Pipeline> action, MethodBase method, params object[] args)
        {
            var result = ExecuteRequest(
                p => { action(p); return default(int); },
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
            CommandContext.RunChecks(pipeline, method, args);

            try
            {
                if (!pipeline.Success) return pipeline;

                pipeline.Data = func(pipeline);
                return pipeline;
            }
            finally { pipeline.Log(); }
        }
    }
}