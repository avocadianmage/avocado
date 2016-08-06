using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Threading;
using UtilityLib.MiscTools;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecutingPipeline
    {
        readonly AutoResetEvent executionDone = new AutoResetEvent(false);

        readonly Runspace runspace;
        Pipeline pipeline;

        public ExecutingPipeline(Runspace runspace)
        {
            this.runspace = runspace;
        }

        /// <summary>
        /// Asynchronous request to stop the running pipeline.
        /// </summary>
        /// <returns>True if the pipeline is being stopped. False otherwise
        /// (ex: the pipeline wasn't running).</returns>
        public bool Stop()
        {
            if (pipeline?.PipelineStateInfo.State == PipelineState.Running)
            {
                pipeline.StopAsync();
                return true;
            }
            return false;
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
            executionDone.WaitOne();
        }

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Failed:
                case PipelineState.Stopped:
                    // Fire event indicating execution of the pipeline is finished.
                    executionDone.Set();
                    break;
            }
        }
    }
}