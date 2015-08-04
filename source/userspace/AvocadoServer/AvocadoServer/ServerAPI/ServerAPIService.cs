using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Linq;
using UtilityLib.WCF;

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
            return Job.JobList.Select(x => x.ToString());
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