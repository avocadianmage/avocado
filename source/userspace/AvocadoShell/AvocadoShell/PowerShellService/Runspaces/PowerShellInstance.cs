using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading.Tasks;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class PowerShellInstance
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;
        public event EventHandler ExitRequested;

        readonly IShellUI shellUI;
        readonly CustomHost psHost;
        readonly ExecutingPipeline executingPipeline;
        readonly Autocomplete autocomplete;

        public PowerShellInstance(IShellUI ui) : this(ui, null) { }

        public PowerShellInstance(IShellUI ui, string remoteComputerName)
        {
            shellUI = ui;

            // Create PowerShell service objects.
            psHost = createHost(ui);
            var powershell = createPowershell(
                ui, createRemoteInfo(remoteComputerName), psHost);
            executingPipeline = createPipeline(powershell.Runspace);
            autocomplete = new Autocomplete(powershell);
        }

        public PSHostRawUserInterface HostRawUI => psHost.UI.RawUI;
        public string RemoteComputerName 
            => executingPipeline.Runspace.ConnectionInfo?.ComputerName;
        public async Task<string> GetWorkingDirectory() => 
            await executingPipeline.GetWorkingDirectory();

        public async Task<IEnumerable<string>> RunBackgroundCommand(
            string command)
            => await executingPipeline.RunBackgroundCommand(command);

        public void InitEnvironment()
        {
            var startupScripts = getSystemStartupScripts()
                .Concat(getUserStartupScripts());
            executingPipeline.ExecuteScripts(startupScripts);
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
        
        public void ExecuteCommand(string cmd) 
            => executingPipeline.ExecuteCommand(cmd);

        public void Stop() => executingPipeline.Stop();

        public async Task<string> GetCompletion(
            string input, int index, bool forward)
            => await autocomplete.GetCompletion(input, index, forward);
        
        WSManConnectionInfo createRemoteInfo(string computerName)
        {
            return computerName == null
                ? null
                : new WSManConnectionInfo { ComputerName = computerName };
        }

        CustomHost createHost(IShellUI ui)
        {
            var host = new CustomHost(ui);
            host.ExitRequested += (s, e) => ExitRequested(this, e);
            return host;
        }

        PowerShell createPowershell(
            IShellUI ui, WSManConnectionInfo remoteInfo, CustomHost host)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = createRunspace(ui, remoteInfo, host);
            return powershell;
        }
        
        Runspace createRunspace(
            IShellUI ui, WSManConnectionInfo remoteInfo, CustomHost host)
        {
            var runspace = remoteInfo == null
                ? RunspaceFactory.CreateRunspace(host)
                : RunspaceFactory.CreateRunspace(
                    remoteInfo, host, TypeTable.LoadDefaultTypeFiles());
            runspace.Open();
            return runspace;
        }

        ExecutingPipeline createPipeline(Runspace runspace)
        {
            var pipeline = new ExecutingPipeline(runspace);
            pipeline.Done += (s, e) => ExecDone(this, e);
            pipeline.ErrorReceived += onErrorReceived;
            return pipeline;
        }

        void onErrorReceived(object sender, string e)
            => shellUI.WriteErrorLine(e);
    }
}