using AvocadoShell.PowerShellService.Modules;
using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class RunspacePipeline
    {
        readonly ResetEventWithData<string> executionDone 
            = new ResetEventWithData<string>();

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
                .Concat(getUserStartupScripts());
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

        IEnumerable<string> getUserStartupScripts()
            => string
                .Join(" ", Environment.GetCommandLineArgs().Skip(1))
                .Yield();

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

        public string ExecuteCommand(string command)
        {
            pipeline = Runspace.CreatePipeline(command, true);
            return executePipeline();
        }

        public string ExecuteScripts(IEnumerable<string> scripts)
        {
            var scriptStr = string.Join(Environment.NewLine, scripts);
            pipeline = Runspace.CreatePipeline(scriptStr);
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
            return executionDone.Block();
        }

        void onPipelineStateChanged(object sender, PipelineStateEventArgs e)
        {
            switch (e.PipelineStateInfo.State)
            {
                case PipelineState.Completed:
                case PipelineState.Failed:
                case PipelineState.Stopped:
                    // Fire event indicating execution of the pipeline is 
                    // finished.
                    executionDone.Signal(e.PipelineStateInfo.Reason?.Message);
                    break;
            }
        }
    }
}