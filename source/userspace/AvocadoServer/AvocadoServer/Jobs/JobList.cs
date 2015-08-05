using System.Collections.Generic;
using System.Linq;
using UtilityLib.WCF;

namespace AvocadoServer.Jobs
{
    sealed class JobList
    {
        readonly Dictionary<int, Job> jobLookup = new Dictionary<int, Job>();

        public IEnumerable<string> JobLookupInfo 
            => jobLookup.Values.Select(x => x.ToString());

        int reserveId(Job job)
        {
            lock (jobLookup)
            {
                // Find an available id.
                var id = 0;
                while (jobLookup.ContainsKey(++id)) ;

                // Create a new entry in the lookup list.
                jobLookup.Add(id, job);

                return id;
            }
        }

        public WCFMessage StartJob(
            string app, 
            string name, 
            int secInterval,
            IEnumerable<string> args)
        {
            // Create job object.
            var job = new Job(app, name, secInterval, args);

            // Verify the job is valid.
            string error;
            if (!job.Verify(out error)) return new WCFMessage(false, error);

            var id = reserveId(job);
            job.Start(id);

            return new WCFMessage(true, $"Job {job} started.");
        }
        
        public WCFMessage KillJob(int id)
        {
            lock (jobLookup)
            {
                // Verify job exists.
                if (!jobLookup.ContainsKey(id))
                {
                    return new WCFMessage(
                        false, 
                        $"Failure killing job: ID {id} not found.");
                }
                var job = jobLookup[id];

                // Kill the job.
                job.Kill();

                // Remove it from the lookup table.
                jobLookup.Remove(id);

                return new WCFMessage(true, $"Job {job} killed.");
            }
        }
    }
}