using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvocadoServer.ServerCore
{
    [DataContract]
    public sealed class Job
    {
        static readonly Dictionary<int, Job> jobLookup 
            = new Dictionary<int, Job>();

        public static void Start(string app, string name, string[] args)
        {
            createJob(app, name).Start(args);
        }

        static Job createJob(string app, string name)
        {
            lock (jobLookup)
            {
                // Find an available id.
                var id = 1;
                while (jobLookup.ContainsKey(id)) id++;

                // Create the new job and add it to the lookup list.
                var job = new Job(id, app, name);
                jobLookup.Add(id, job);
                return job;
            }
        }

        readonly int id;
        readonly string app;
        readonly string name;

        Job(int id, string app, string name)
        {
            this.id = id;
            this.app = app;
            this.name = name;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1} (id:{2}) ", app, name, id);
        }

        public void Start(string[] arg)
        {
            Logger.WriteLogItem("Job started: {0}", ToString());
        }
    }
}