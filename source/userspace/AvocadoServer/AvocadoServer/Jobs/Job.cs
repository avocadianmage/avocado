using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
using AvocadoUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoServer.Jobs
{
    public sealed class Job
    {
        readonly string app;
        readonly string name;
        readonly int secInterval;
        readonly IEnumerable<string> args;

        int id;
        CancellationTokenSource tokenSource;

        public Job(
            string app, 
            string name, 
            int secInterval, 
            IEnumerable<string> args)
        {
            this.app = app;
            this.name = name;
            this.secInterval = secInterval;
            this.args = args;
        }

        public bool Verify(out string error)
        {
            // Verify file exists.
            if (!File.Exists(exePath))
            {
                // Output that the job failed to start.
                error = $"Failure starting job: server file [{exePath}] does not exist.";
                return false;
            }

            error = null;
            return true;
        }

        public override string ToString() 
            => $"{app}.{name}({string.Join(" ", args)})";

        public void Start(int id)
        {
            this.id = id;
            tokenSource = new CancellationTokenSource();
            Task.Run(schedulerThread, tokenSource.Token);
        }

        public void Kill() => tokenSource.Cancel();

        string exePath => Path.Combine(app, $"{name}.exe");

        async Task schedulerThread()
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                // Dispatch an execution of the job.
                dispatchedThread().RunAsync();
                
                // Wait for the specified interval between dispatches.
                const int MsInSec = 1000;
                await Task.Delay(secInterval * MsInSec);
            }
        }

        //TODO: terminate running process?
        async Task dispatchedThread()
        {
            var proc = new ManagedProcess(exePath, args.ToArray());
            proc.OutputReceived += (s, e) => Logger.WriteLine(this, e.Data);
            proc.ErrorReceived += (s, e) => Logger.WriteErrorLine(this, e.Data);
            await proc.RunBackgroundLive();
        }
        
        public XmlJob ToXml() => new XmlJob
        {
            App = app,
            Name = name,
            SecInterval = secInterval,
            Args = args.ToList()
        };
    }
}