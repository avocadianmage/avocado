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
        public event EventHandler<string> ErrorReceived;

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
            // Have the our host format the output.
            pipeline.Commands.Add("Out-Default");

            // Subscribe to the pipeline lifetime events.
            pipeline.StateChanged += onPipelineStateChanged;

            // Execute the pipeline.
            pipeline.InvokeAsync();
        }

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            string error;
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Failed:
                    error = e.PipelineStateInfo.Reason?.Message;
                    break;
                case PipelineState.Stopped:
                    error = "[break]";
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
                ErrorReceived(this, exc.Message);
            }
            return result?.Select(l => l.ToString());
        }
    }
}