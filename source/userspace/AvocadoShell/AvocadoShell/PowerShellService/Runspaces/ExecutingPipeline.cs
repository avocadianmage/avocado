using AvocadoShell.PowerShellService.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        public event EventHandler<ExecDoneEventArgs> Done;
        public event EventHandler<IEnumerable<string>> OutputReceived;
        public event EventHandler<IEnumerable<string>> ErrorReceived;

        public Runspace Runspace { get; }

        Pipeline pipeline;

        public ExecutingPipeline(Runspace runspace)
        {
            Runspace = runspace;
        }

        public void Stop()
        {
            // Only stop the pipeline if it is running.
            if (pipeline?.PipelineStateInfo.State == PipelineState.Running)
            {
                pipeline.StopAsync();
            }
        }

        public void ExecuteCommand(string command)
        {
            pipeline = Runspace.CreatePipeline(command, true);
            executePipeline();
        }

        public void ExecuteScripts(IEnumerable<string> scripts)
        {
            pipeline = Runspace.CreatePipeline();
            scripts.ForEach(pipeline.Commands.AddScript);
            executePipeline();
        }

        void executePipeline()
        {
            pipeline.StateChanged += onPipelineStateChanged;
            pipeline.Output.DataReady += onOutputDataReady;
            pipeline.Error.DataReady += onErrorDataReady;
            pipeline.InvokeAsync();
        }

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            string error;
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                    error = null;
                    break;
                case PipelineState.Failed:
                    error = e.PipelineStateInfo.Reason.Message;
                    break;
                case PipelineState.Stopped:
                    error = "Execution aborted.";
                    break;
                default: return;
            }

            // Reset the pipeline.
            pipeline = null;

            // Fire event indicating execution of the pipeline is finished.
            Done(this, new ExecDoneEventArgs(error));
        }

        public async Task<string> GetWorkingDirectory()
        {
            // SessionStateProxy properties are not supported in remote 
            // runspaces, so we must manually get the working directory by
            // running a PowerShell command.
            if (Runspace.RunspaceIsRemote)
            {
                var result = await RunBackgroundCommand(
                    "$PWD.Path.Replace($HOME, '~')");
                return result.First();
            }

            var homeDir = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);
            return Runspace.SessionStateProxy
                .Path.CurrentLocation.Path
                .Replace(homeDir, "~");
        }

        public async Task<IEnumerable<string>> RunBackgroundCommand(
            string command)
        {
            IEnumerable<PSObject> result = null;
            try
            {
                result = await Task.Run(
                    () => Runspace.CreatePipeline(command).Invoke());
            }
            catch (RuntimeException exc)
            {
                ErrorReceived(this, exc.Message.Yield());
            }
            return result?.Select(l => l.ToString());
        }

        void onOutputDataReady(object sender, EventArgs e)
        {
            var outputList = readData(
                sender as PipelineReader<PSObject>,
                o => OutputFormatter.FormatPSObject(o));
            OutputReceived(this, outputList);
        }
        
        void onErrorDataReady(object sender, EventArgs e)
        {
            var errorList = readData(
                sender as PipelineReader<object>, 
                o => o.ToString().Yield());
            ErrorReceived(this, errorList);
        }

        IEnumerable<string> readData<T>(
            PipelineReader<T> reader, 
            Func<T, IEnumerable<string>> format)
        {
            var data = new List<string>();
            while (reader.Count > 0) data.AddRange(format(reader.Read()));
            return data;
        }
    }
}