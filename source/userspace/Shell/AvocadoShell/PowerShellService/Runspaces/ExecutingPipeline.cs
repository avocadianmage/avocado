using AvocadoShell.PowerShellService.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        public event EventHandler<ExecDoneEventArgs> Done;
        public event EventHandler<IEnumerable<string>> OutputReceived;
        public event EventHandler<IEnumerable<string>> ErrorReceived;

        Pipeline pipeline;

        public Runspace Runspace => pipeline.Runspace;

        public ExecutingPipeline(Runspace runspace)
        {
            pipeline = runspace.CreatePipeline();
        }

        public void Stop() => pipeline.StopAsync();

        public void AddScript(string script)
            => pipeline.Commands.AddScript(script);

        public void Execute()
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
            
            // Fire event indicating execution of the pipeline is finished.
            Done(this, new ExecDoneEventArgs(GetWorkingDirectory(), error));

            // Reset the pipeline.
            pipeline = pipeline.Runspace.CreatePipeline();
        }

        public string GetWorkingDirectory()
        {
            // SessionStateProxy properties are not supported in remote 
            // runspaces, so we must manually get the working directory by
            // running a PowerShell command.
            if (pipeline.Runspace.RunspaceIsRemote)
            {
                return pipeline.Runspace
                    .CreatePipeline("$PWD.Path.Replace($HOME, '~')")
                    .Invoke().First().ToString();
            }

            var homeDir = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);
            return pipeline.Runspace.SessionStateProxy
                .Path.CurrentLocation.Path
                .Replace(homeDir, "~");
        }

        void onOutputDataReady(object sender, EventArgs e)
        {
            var outputList = new List<string>();

            var reader = sender as PipelineReader<PSObject>;
            while (reader.Count > 0)
            {
                OutputFormatter
                    .FormatPSObject(reader.Read())
                    .ForEach(outputList.Add);
            }

            OutputReceived(this, outputList);
        }
        
        void onErrorDataReady(object sender, EventArgs e)
        {
            var errorList = new List<string>();

            var reader = sender as PipelineReader<object>;
            while (reader.Count > 0)
            {
                errorList.Add(reader.Read().ToString());
            }

            ErrorReceived(this, errorList);
        }
    }
}