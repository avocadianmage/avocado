using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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
        readonly ExecutingPipeline pipeline;
        readonly Autocomplete autocomplete;

        public PowerShellInstance(IShellUI ui) : this(ui, null) { }

        public PowerShellInstance(IShellUI ui, string remoteComputerName)
        {
            shellUI = ui;

            // Create PowerShell service objects.
            var remoteInfo = createRemoteInfo(remoteComputerName);
            var powershell = createPowershell(ui, remoteInfo);
            pipeline = createPipeline(powershell.Runspace);

            // No support for autocompletion while remoting.
            autocomplete = new Autocomplete(powershell);
        }

        public string RemoteComputerName 
            => pipeline.Runspace.ConnectionInfo?.ComputerName;
        public async Task<string> GetWorkingDirectory() => 
            await pipeline.GetWorkingDirectory();

        public async Task<IEnumerable<string>> RunBackgroundCommand(
            string command)
            => await pipeline.RunBackgroundCommand(command);

        public void InitEnvironment()
        {
            var startupScripts = getSystemStartupScripts()
                .Concat(getUserStartupScripts());
            pipeline.ExecuteScripts(startupScripts);
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
        
        public void ExecuteCommand(string cmd) => pipeline.ExecuteCommand(cmd);

        public void Stop() => pipeline.Stop();

        public async Task<string> GetCompletion(
            string input, int index, bool forward)
            => await autocomplete.GetCompletion(input, index, forward);
        
        WSManConnectionInfo createRemoteInfo(string computerName)
        {
            return computerName == null
                ? null
                : new WSManConnectionInfo { ComputerName = computerName };
        }
        
        PowerShell createPowershell(IShellUI ui, WSManConnectionInfo remoteInfo)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = createRunspace(ui, remoteInfo);
            return powershell;
        }
        
        Runspace createRunspace(IShellUI ui, WSManConnectionInfo remoteInfo)
        {
            // Initialize custom PowerShell host.
            var host = new CustomHost(ui);
            host.ExitRequested += (s, e) => ExitRequested(this, e);

            // Initialize local or remote runspace.
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