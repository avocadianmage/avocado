using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerAPI;
using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                var pipeline = new Pipeline();
                startJobCore(pipeline, job);
                pipeline.Log();
            }
        }

        void saveJobs() => Task.Run(() => JobSerializer.Save(jobTable.Values));
        
        public void StartJob(
            Pipeline pipeline,
            string filename, 
            int secInterval,
            IEnumerable<string> args)
        {
            // Create job object.
            var job = new Job(filename, secInterval, args);

            startJobCore(pipeline, job);

            // If the job successfully started, save it to disk.
            if (pipeline.Success) saveJobs();
        }

        void startJobCore(Pipeline pipeline, Job job)
        {
            reserveId(job);
            job.Start(pipeline);
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