using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
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
        readonly string filename;
        readonly int secInterval;
        readonly IEnumerable<string> args;

        int id;
        CancellationTokenSource tokenSource;

        public Job(string filename, int secInterval, IEnumerable<string> args)
        {
            this.filename = Path.GetFullPath(filename);
            this.secInterval = secInterval;
            this.args = args;
        }

        public bool Verify(out string error)
        {
            // Verify file exists.
            if (!File.Exists(filename))
            {
                // Output that the job failed to start.
                error = $"Failure starting job: server file [{filename}] does not exist.";
                return false;
            }

            error = null;
            return true;
        }

        public override string ToString() 
            => $"{filename}({string.Join(", ", args)})";

        public void Start(int id)
        {
            this.id = id;
            tokenSource = new CancellationTokenSource();
            Task.Run(schedulerThread, tokenSource.Token);
        }

        public void Kill() => tokenSource.Cancel();

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
            var proc = new ManagedProcess(filename, args.ToArray());
            proc.OutputReceived += (s, e) => Logger.WriteLine(this, e.Data);
            proc.ErrorReceived += (s, e) => Logger.WriteErrorLine(this, e.Data);
            await proc.RunBackgroundLive();
        }
        
        public XmlJob ToXml() => new XmlJob
        {
            Filename = filename,
            SecInterval = secInterval,
            Args = args.ToList()
        };
    }
}