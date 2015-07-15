using AvocadoServer.ServerCore;
using System.Collections.Generic;

namespace AvocadoServer.ServerAPI
{
    public class ServerAPIService : IServerAPI
    {
        public bool Ping()
        {
            return true;
        }

        public IEnumerable<string> GetJobs()
        {
            return new List<string> { "Hello world!" };
        }

        public void RunJob(string app, string name, params string[] args)
        {
            Logger.WriteLogItem("Job {0}.{1} was started.", app, name);
        }

        public void KillJob(int id)
        {

        }
    }
}