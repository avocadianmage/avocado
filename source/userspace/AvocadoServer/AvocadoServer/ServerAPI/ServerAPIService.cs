using AvocadoServer.ServerCore;
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
            var pipeline = new Pipeline<IEnumerable<string>>();
            CommandContext.RunChecks(pipeline, MethodBase.GetCurrentMethod());

            try
            {
                if (!pipeline.Success) return pipeline;

                // Execute logic.
                pipeline.Data = EntryPoint.Jobs.GetJobTableInfo();
                return pipeline;
            }
            finally { pipeline.Log(); }
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public Pipeline RunJob(
            string filename, 
            int secInterval, 
            string[] args)
        {
            var pipeline = new Pipeline();
            CommandContext.RunChecks(
                pipeline,
                MethodBase.GetCurrentMethod(),
                filename,
                secInterval,
                args);

            try
            {
                if (!pipeline.Success) return pipeline;

                // Execute logic.
                EntryPoint.Jobs.StartJob(pipeline, filename, secInterval, args);
                return pipeline;
            }
            finally { pipeline.Log(); }
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public Pipeline KillJob(int id)
        {
            var pipeline = new Pipeline();
            CommandContext.RunChecks(
                pipeline, MethodBase.GetCurrentMethod(), id);

            try
            {
                if (!pipeline.Success) return pipeline;

                // Execute logic.
                EntryPoint.Jobs.KillJob(pipeline, id);
                return pipeline;
            }
            finally { pipeline.Log(); }
        }
    }
}