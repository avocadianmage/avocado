using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerAPI;
using System.Collections.Generic;
using System.Linq;

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
            var jobs = JobSerializer.Load();
            if (jobs == null) return;

            // Start all jobs that were deserialized.
            foreach (var job in jobs) startJobCore(job);
        }

        void saveJobs() => JobSerializer.Save(jobTable.Values);
        
        public void StartJob(
            string filename, 
            int secInterval,
            IEnumerable<string> args)
        {
            startJobCore(new Job(filename, secInterval, args));
            saveJobs();
        }

        void startJobCore(Job job)
        {
            reserveId(job);
            job.Start();
        }

        void reserveId(Job job)
        {
            lock (jobTable)
            {
                // Find an available id.
                var id = 0;
                while (jobTable.ContainsKey(++id)) ;

                // Create a new entry in the job table.
                jobTable.Add(id, job);
            }
        }

        public void KillJob(Pipeline pipeline, int id)
        {
            lock (jobTable)
            {
                // Verify job exists.
                if (!jobTable.ContainsKey(id))
                {
                    pipeline.Success = false;
                    pipeline.Message 
                        = $"Failure killing job: ID {id} not found.";
                    return;
                }
                var job = jobTable[id];

                // Kill the job.
                job.Kill();

                // Remove it from the job table.
                jobTable.Remove(id);

                // Update saved jobs on disk.
                saveJobs();

                pipeline.Success = true;
                pipeline.Message = $"Job {job} terminated.";
            }
        }
    }
}