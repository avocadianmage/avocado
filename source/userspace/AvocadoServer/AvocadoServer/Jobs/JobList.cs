using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilityLib.MiscTools;

namespace AvocadoServer.Jobs
{
    sealed class JobList
    {
        readonly Dictionary<int, Job> jobTable = new Dictionary<int, Job>();

        public string GetJobTableInfo()
        {
            if (!jobTable.Any()) return null;

            var padding = jobTable.Keys.Max().ToString().Length;
            return jobTable
                .Select(pair =>
                    $"{pair.Key.ToString().PadLeft(padding)} - [{pair.Value}]")
                .Aggregate((a, l) => a + Environment.NewLine + l);
        }
        
        public void RestoreFromDisk()
        {
            // Load saved jobs from disk and start them.
            new JobSerializer().Load()?.ForEach(job =>
            {
                reserveId(job);
                job.Start();
            });
        }

        void saveJobs()
            => Task.Run(() => new JobSerializer().Save(jobTable.Values));
        
        public void StartJob(
            string workingDirectory, string filename, int? secInterval)
        {
            var job = new Job(workingDirectory, filename, secInterval);
            if (secInterval.HasValue)
            {
                reserveId(job);
                saveJobs();
            }
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