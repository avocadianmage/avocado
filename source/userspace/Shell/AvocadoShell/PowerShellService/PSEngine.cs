using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Modules;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoShell.PowerShellService
{
    sealed class PSEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;

        readonly IShellUI shellUI;
        readonly ExecutingPipeline pipeline;
        readonly Autocomplete autocomplete;

        public PSEngine(IShellUI ui)
        {
            shellUI = ui;

            var powershell = createPowershell(ui);
            pipeline = createPipeline(powershell.Runspace);

            // No support for autocompletion while remoting.
            if (!isRemote) autocomplete = new Autocomplete(powershell);
        }

        bool isRemote => pipeline.Runspace.RunspaceIsRemote;

        public async Task InitEnvironment()
        {
            shellUI.WriteOutputLine($"Booting avocado [v{Config.Version}]");
            await doWork(
                "Starting autocompletion service",
                autocomplete?.InitializeService());
            await doWork("Running startup scripts", runStartupScripts);
        }

        async Task doWork(string message, Action action)
            => await doWork(message, Task.Run(action));

        async Task doWork(string message, Task work)
        { 
            if (work == null) return;
            shellUI.WriteCustom($"{message}...", Config.SystemFontBrush, false);
            await work;
            shellUI.WriteOutputLine("Done.");
        }

        void runStartupScripts()
        {
            ExecuteCommand(
                EnvUtils.GetEmbeddedText("AvocadoShell.Assets.startup.ps1"));
        }

        public void ExecuteCommand(string cmd)
        {
            pipeline.AddScript(cmd);
            pipeline.Execute();
        }

        public void Stop() => pipeline.Stop();

        public async Task<string> GetCompletion(
            string input,
            int index,
            bool forward)
        {
            // No support for autocompletion while remoting.
            if (isRemote) return null;
            return await autocomplete.GetCompletion(input, index, forward);
        }

        PowerShell createPowershell(IShellUI ui)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = createRunspace(ui);
            return powershell;
        }

        Runspace createRunspace(IShellUI ui)
        {
            var host = new CustomHost(ui);
            var runspace = RunspaceFactory.CreateRunspace(host);
            runspace.Open();
            return runspace;
        }

        ExecutingPipeline createPipeline(Runspace runspace)
        {
            var pipeline = new ExecutingPipeline(runspace);
            pipeline.Done += (s, e) => ExecDone(this, e);
            pipeline.OutputReceived += onOutputReceived;
            pipeline.ErrorReceived += onErrorReceived;
            return pipeline;
        }

        void onOutputReceived(object sender, IEnumerable<string> e)
            => e.ForEach(shellUI.WriteOutputLine);

        void onErrorReceived(object sender, IEnumerable<string> e)
            => e.ForEach(shellUI.WriteErrorLine);
    }
}