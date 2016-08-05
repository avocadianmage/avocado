using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        readonly ResetEventWithData<string> mutex 
            = new ResetEventWithData<string>();

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

        public string ExecuteCommand(string command)
        {
            pipeline = runspace.CreatePipeline(command, true);
            return executePipeline();
        }

        public string ExecuteScripts(IEnumerable<string> scripts)
        {
            pipeline = runspace.CreatePipeline();
            scripts.ForEach(pipeline.Commands.AddScript);
            return executePipeline();
        }

        string executePipeline()
        {
            // Have the our host format the output.
            pipeline.Commands.Add("Out-Default");

            // Subscribe to the pipeline lifetime events.
            pipeline.StateChanged += onPipelineStateChanged;

            // Execute the pipeline.
            pipeline.InvokeAsync();

            // Wait for the pipeline to finish and return any error output.
            return mutex.Block();
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
            mutex.Signal(error);
        }
    }
}