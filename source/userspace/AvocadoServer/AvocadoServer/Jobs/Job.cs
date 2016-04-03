using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
using System.Collections.Generic;
using System.ComponentModel;
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
        
        readonly CancellationTokenSource tokenSource 
            = new CancellationTokenSource();

        public Job(string filename, int secInterval, IEnumerable<string> args)
        {
            this.filename = filename;
            this.secInterval = secInterval;
            this.args = args;
        }

        public override string ToString()
        {
            var str = filename;
            if (args.Any()) str += $"({string.Join(", ", args)})";
            return str;
        }
        
        public void Start() => Task.Run(schedulerThread, tokenSource.Token);

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
            // Initialize the process.
            var proc = new ManagedProcess(filename, args.ToArray());
            proc.OutputReceived += (s, e) => Logger.WriteLine(this, e.Data);
            proc.ErrorReceived += (s, e) => Logger.WriteErrorLine(this, e.Data);

            // Run the process, catching any failures.
            Logger.WriteLine(this, "Starting instance.");
            try { await proc.RunBackgroundLive(); }
            catch (Win32Exception exc)
            {
                Logger.WriteErrorLine(this, exc.Message);
            }
        }
        
        public XmlJob ToXml() => new XmlJob
        {
            Filename = filename,
            SecInterval = secInterval,
            Args = args.ToList()
        };
    }
}