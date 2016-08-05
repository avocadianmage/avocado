using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Threading;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        readonly AutoResetEvent mutex = new AutoResetEvent(false);

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

            // Wait for the pipeline to finish and return any error output.
            mutex.WaitOne();
        }

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Failed:
                case PipelineState.Stopped:
                    break;

                default: return;
            }

            // Reset the pipeline.
            pipeline = null;

            // Fire event indicating execution of the pipeline is finished.
            mutex.Set();
        }
    }
}