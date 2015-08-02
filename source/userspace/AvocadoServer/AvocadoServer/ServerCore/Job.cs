using AvocadoUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

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
        readonly int secInterval;
        readonly IEnumerable<string> args;

        public Job(
            string app, 
            string name, 
            int secInterval, 
            IEnumerable<string> args)
        {
            id = reserveId();
            this.app = app;
            this.name = name;
            this.secInterval = secInterval;
            this.args = args;
        }

        int reserveId()
        {
            lock (jobLookup)
            {
                // Find an available id.
                var id = 0;
                while (jobLookup.ContainsKey(++id));

                // Create a new entry in the lookup list.
                jobLookup.Add(id, this);

                return id;
            }
        }

        public override string ToString() => $"{app}.{name}:{id}";

        public void Start()
        {
            Logger.WriteLine($"Job {ToString()} started.");
            Task.Run(schedulerThread);
        }

        string exePath
            => Path.Combine(RootDir.Avocado.Apps.Val, app, $"{name}.exe");

        async Task schedulerThread()
        {
            while (true) //TODO - listen for killing
            {
                // Dispatch an execution of the job.
                dispatchedThread().RunAsync();
                
                // Wait for the specified interval between dispatches.
                const int MsInSec = 1000;
                await Task.Delay(secInterval * MsInSec);
            }
        }

        async Task dispatchedThread()
        {
            var proc = new ManagedProcess(exePath, args.ToArray());
            proc.OutputReceived += (s, e) => Logger.WriteLine(this, e.Data);
            proc.ErrorReceived += (s, e) => Logger.WriteErrorLine(this, e.Data);
            await proc.RunBackgroundLive();
        }
    }
}