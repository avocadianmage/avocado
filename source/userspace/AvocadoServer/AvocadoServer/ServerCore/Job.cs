using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvocadoServer.ServerCore
{
    [DataContract]
    public sealed class Job
    {
        static readonly Dictionary<int, Job> jobLookup 
            = new Dictionary<int, Job>();

        readonly int id;
        readonly string app;
        readonly string name;
        readonly IEnumerable<string> args;

        public Job(string app, string name, IEnumerable<string> args)
        {
            id = reserveId();
            this.app = app;
            this.name = name;
            this.args = args;
        }

        int reserveId()
        {
            lock (jobLookup)
            {
                // Find an available id.
                var id = 1;
                while (jobLookup.ContainsKey(id)) id++;

                // Create a new entry in the lookup list.
                jobLookup.Add(id, this);

                return id;
            }
        }

        public override string ToString() => $"{app}.{name}:{id}";

        public void Start()
        {
            Logger.WriteLine($"Job {ToString()} started.");
        }
    }
}