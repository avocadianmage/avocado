using AvocadoServer.Jobs.Serialization;
using AvocadoServer.ServerCore;
using StandardLibrary.Extensions;
using StandardLibrary.Processes;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace AvocadoServer.Jobs
{
    public sealed class Job
    {
        readonly string workingDirectory;
        readonly string filename;
        readonly int secInterval;
        
        readonly CancellationTokenSource tokenSource 
            = new CancellationTokenSource();

        public Job(string workingDirectory, string filename, int secInterval)
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
                Task.Run(() => dispatchedThread()).RunAsync();

                // If no interval is specified, only execute the job once.
                if (secInterval <= 0) break;

                // Wait for the specified interval between dispatches.
                const int MsInSec = 1000;
                await Task.Delay(secInterval * MsInSec).ConfigureAwait(false);
            }
        }

        //TODO: terminate running process?
        void dispatchedThread()
        {
            // Initialize the process.
            var proc = new ManagedProcess("PowerShell", filename);
            proc.WorkingDirectory = workingDirectory;
            proc.OutputReceived += (s, e) => Logger.WriteLine(this, e.Data);
            proc.ErrorReceived += (s, e) => Logger.WriteErrorLine(this, e.Data);

            // Run the process, catching any failures.
            Logger.WriteLine(this, "Starting instance.");
            try { proc.RunBackgroundLive(); }
            catch (Win32Exception exc)
            {
                Logger.WriteErrorLine(this, exc.Message);
            }
        }
        
        public XmlJob ToXml() => new XmlJob
        {
            WorkingDirectory = workingDirectory,
            Filename = filename,
            SecInterval = secInterval
        };
    }
}