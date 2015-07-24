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

        public IEnumerable<Job> GetJobs()
        {
            return null;
        }

        public void RunJob(string app, string name, string[] args)
        {
            new Job(app, name, args).Start();
        }

        public void KillJob(int id)
        {

        }
    }
}