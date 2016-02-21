using AvocadoServer.ServerCore;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using UtilityLib.WCF;

namespace AvocadoServer.ServerAPI
{
    public sealed class ServerAPIService : IServerAPI
    {
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public bool Ping() => true;

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public IEnumerable<string> GetJobs()
        {
            Console.Out.LogLine(nameof(GetJobs), null);
            return EntryPoint.Jobs.GetJobTableInfo();
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public WCFMessage RunJob(
            string app, 
            string name, 
            int secInterval, 
            string[] args)
        {
            var msg = EntryPoint.Jobs.StartJob(app, name, secInterval, args);
            Logger.WriteWCFMessage(msg);
            return msg;
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public WCFMessage KillJob(int id)
        {
            var msg = EntryPoint.Jobs.KillJob(id);
            Logger.WriteWCFMessage(msg);
            return msg;
        }
    }
}