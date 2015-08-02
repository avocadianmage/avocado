using AvocadoServer.ServerCore;
using System.Collections.Generic;
using UtilityLib.WCF;

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

        public WCFMessage RunJob(
            string app, 
            string name, 
            int secInterval, 
            string[] args)
        {
            // Create new job.
            var job = new Job(app, name, secInterval, args);

            // Start job.
            string output;
            var success = job.Start(out output);

            // Return result message.
            return new WCFMessage(success, output);
        }

        public void KillJob(int id)
        {
            
        }
    }
}