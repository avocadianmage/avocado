using AvocadoShell.PowerShellService.Utilities;
using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace AvocadoShell.PowerShellService
{
    sealed class RunspacePipeline
    {
        readonly ResetEventWithData<PipelineStateInfo> executionDone 
            = new ResetEventWithData<PipelineStateInfo>();
        Pipeline pipeline;

        public Runspace Runspace { get; }
        public Autocomplete Autocomplete { get; }

        public RunspacePipeline(Runspace runspace)
        {
            Runspace = runspace;
            Autocomplete = new Autocomplete(runspace);
        }

        public string InitEnvironment()
        {
            var startupScripts = getSystemStartupScripts()
                .Concat(getParameterScripts());
            return ExecuteScripts(startupScripts);
        }

        IEnumerable<string> getSystemStartupScripts()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm
                .GetManifestResourceNames()
                .Where(r => r.EndsWith(".ps1"))
                .Select(r => EnvUtils.GetEmbeddedText(asm, r));
        }

        IEnumerable<string> getParameterScripts()
            => string.Join(" ", EnvUtils.GetArgs()).Yield();

        /// <summary>
        /// Request to stop the running pipeline.
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

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Stopped:
                    executionDone.Signal(e.PipelineStateInfo);
                    break;
            }
        }

        public string ExecuteCommand(string command)
        {
            return executePipeline(command, true);
        }

        public string ExecuteScripts(IEnumerable<string> scripts)
        {
            var scriptStr = string.Join(Environment.NewLine, scripts);
            return executePipeline(scriptStr, false);
        }

        string executePipeline(string command, bool addToHistory)
        {
            // Create the new pipeline.
            pipeline = Runspace.CreatePipeline(command, addToHistory);
            pipeline.StateChanged += onPipelineStateChanged;

            // Have the our host format the output.
            pipeline.Commands.Add("Out-Default");

            // Execute the pipeline.
            pipeline.InvokeAsync();

            // Wait for the pipeline to finish and return any error output.
            return executionDone.Block().Reason?.Message;
        }
    }
}
