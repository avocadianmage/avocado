using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
using AvocadoUtilities.CommandLine;
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
        readonly string workingDirectory;
        readonly string filename;
        readonly int? secInterval;
        
        readonly CancellationTokenSource tokenSource 
            = new CancellationTokenSource();

        public Job(string workingDirectory, string filename, int? secInterval)
        {
            this.workingDirectory = workingDirectory;
            this.filename = filename;
            this.secInterval = secInterval;
        }

        public override string ToString() => filename;
        
        public void Start() => Task.Run(schedulerThread, tokenSource.Token);

        public void Kill() => tokenSource.Cancel();

        async Task schedulerThread()
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                // Dispatch an execution of the job.
                dispatchedThread().RunAsync();

                // If no interval is specified, only execute the job once.
                if (!secInterval.HasValue) break;

                // Wait for the specified interval between dispatches.
                const int MsInSec = 1000;
                await Task.Delay(secInterval.Value * MsInSec);
            }
        }

        //TODO: terminate running process?
        async Task dispatchedThread()
        {
            var args = new Arguments(filename);

            // Initialize the process.
            var proc = new ManagedProcess(
                args.PopArg(), args.PopRemainingArgs().ToArray());
            proc.WorkingDirectory = workingDirectory;
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
            WorkingDirectory = workingDirectory,
            Filename = filename,
            SecInterval = secInterval.Value
        };
    }
}