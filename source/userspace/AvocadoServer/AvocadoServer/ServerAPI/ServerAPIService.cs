using AvocadoServer.ServerCore;
using System.Collections.Generic;
using UtilityLib.WCF;

namespace AvocadoServer.ServerAPI
{
    public sealed class ServerAPIService : IServerAPI
    {
        public bool Ping()
        {
            return true;
        }

        public IEnumerable<string> GetJobs()
        {
            return EntryPoint.Jobs.JobLookupInfo;
        }

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

        public WCFMessage KillJob(int id)
        {
            var msg = EntryPoint.Jobs.KillJob(id);
            Logger.WriteWCFMessage(msg);
            return msg;
        }
    }
}