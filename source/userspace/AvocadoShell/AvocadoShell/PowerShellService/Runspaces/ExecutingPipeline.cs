using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        public event EventHandler<ExecDoneEventArgs> Done;

        readonly Runspace runspace;
        Pipeline pipeline;

        public ExecutingPipeline(Runspace runspace)
        {
            this.runspace = runspace;
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
            pipeline = runspace.CreatePipeline(command, true);
            executePipeline();
        }

        public void ExecuteScripts(IEnumerable<string> scripts)
        {
            pipeline = runspace.CreatePipeline();
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
    }
}