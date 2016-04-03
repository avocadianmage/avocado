using AvocadoServer.ServerCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;

namespace AvocadoServer.ServerAPI
{
    public sealed class ServerAPIService : IServerAPI
    {
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public bool Ping() => true;

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [AllowedClient(ClientType.LAN)]
        public Pipeline<IEnumerable<string>> GetJobs()
        {
            return executeRequest(
                p => EntryPoint.Jobs.GetJobTableInfo(), 
                MethodBase.GetCurrentMethod());
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public Pipeline RunJob(
            string filename, 
            int secInterval, 
            string[] args)
        {
            return executeRequest(
                p => EntryPoint.Jobs.StartJob(p, filename, secInterval, args), 
                MethodBase.GetCurrentMethod(), 
                filename, secInterval, args);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [AllowedClient(ClientType.LAN)]
        public Pipeline KillJob(int id)
        {
            return executeRequest(
                p => EntryPoint.Jobs.KillJob(p, id),
                MethodBase.GetCurrentMethod(), id);
        }

        Pipeline executeRequest(
            Action<Pipeline> action, MethodBase method, params object[] args)
        {
            var result = executeRequest(
                p => { action(p); return default(int); },
                method,
                args);
            return new Pipeline
            {
                Success = result.Success,
                Message = result.Message
            };
        }

        Pipeline<T> executeRequest<T>(
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