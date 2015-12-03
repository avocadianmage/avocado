using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilityLib.WCF;

namespace AvocadoServer.Jobs
{
    sealed class JobList
    {
        readonly Dictionary<int, Job> jobTable = new Dictionary<int, Job>();

        public IEnumerable<string> GetJobTableInfo()
            => jobTable.Select(pair => $"{pair.Key}:{pair.Value}");
        
        public void RestoreFromDisk()
        {
            // Load saved jobs from disk.
            var jobs = JobSerializer.Load().Result;
            if (jobs == null) return;

            // Start all jobs that were deserialized.
            foreach (var job in jobs)
            {
                var msg = startJobCore(job);
                Logger.WriteWCFMessage(msg);
            }
        }

        void saveJobs() => Task.Run(() => JobSerializer.Save(jobTable.Values));
        
        public WCFMessage StartJob(
            string app, 
            string name, 
            int secInterval,
            IEnumerable<string> args)
        {
            // Create job object.
            var job = new Job(app, name, secInterval, args);

            var msg = startJobCore(job);

            // If the job successfully started, save it to disk.
            if (msg.Success) saveJobs();

            return msg;
        }

        WCFMessage startJobCore(Job job)
        {
            // Verify the job is valid.
            string error;
            if (!job.Verify(out error)) return new WCFMessage(false, error);
            
            var id = reserveId(job);
            job.Start(id);

            return new WCFMessage(true, $"Job {job} started.");
        }

        int reserveId(Job job)
        {
            lock (jobTable)
            {
                // Find an available id.
                var id = 0;
                while (jobTable.ContainsKey(++id)) ;

                // Create a new entry in the job table.
                jobTable.Add(id, job);

                return id;
            }
        }

        public WCFMessage KillJob(int id)
        {
            lock (jobTable)
            {
                // Verify job exists.
                if (!jobTable.ContainsKey(id))
                {
                    return new WCFMessage(
                        false, 
                        $"Failure killing job: ID {id} not found.");
                }
                var job = jobTable[id];

                // Kill the job.
                job.Kill();

                // Remove it from the job table.
                jobTable.Remove(id);

                // Update saved jobs on disk.
                saveJobs();

                return new WCFMessage(true, $"Job {job} killed.");
            }
        }
    }
}