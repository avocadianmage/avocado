using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using static AvocadoServer.ServerAPI.ServiceLogic;

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
            return ExecuteRequest(
                p => EntryPoint.Jobs.GetJobTableInfo(), 
                MethodBase.GetCurrentMethod());
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public Pipeline RunJob(string filename, int secInterval, string[] args)
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.StartJob(p, filename, secInterval, args), 
                MethodBase.GetCurrentMethod(), 
                filename, secInterval, args);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [AllowedClient(ClientType.LAN)]
        public Pipeline KillJob(int id)
        {
            return ExecuteRequest(
                p => EntryPoint.Jobs.KillJob(p, id),
                MethodBase.GetCurrentMethod(), id);
        }
    }
}