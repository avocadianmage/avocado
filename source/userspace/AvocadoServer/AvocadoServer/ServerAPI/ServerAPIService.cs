using AvocadoServer.ServerCore;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            // Log action the client is requesting.
            Console.Out.LogLine(MethodBase.GetCurrentMethod());

            return EntryPoint.Jobs.GetJobTableInfo();
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public WCFMessage RunJob(
            string app, 
            string name, 
            int secInterval, 
            string[] args)
        {
            // Log action the client is requesting.
            Console.Out.LogLine(
                MethodBase.GetCurrentMethod(), app, name, secInterval, args);

            var msg = EntryPoint.Jobs.StartJob(app, name, secInterval, args);
            Logger.WriteWCFMessage(msg);
            return msg;
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public WCFMessage KillJob(int id)
        {
            // Log action the client is requesting.
            Console.Out.LogLine(MethodBase.GetCurrentMethod(), id);

            var msg = EntryPoint.Jobs.KillJob(id);
            Logger.WriteWCFMessage(msg);
            return msg;
        }
    }
}