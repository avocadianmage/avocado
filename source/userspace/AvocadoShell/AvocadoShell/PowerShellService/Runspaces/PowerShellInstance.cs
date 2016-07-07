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

        readonly ExecutingPipeline executingPipeline;
        readonly Autocomplete autocomplete;

        public PowerShellInstance(CustomHost host) : this(host, null) { }

        public PowerShellInstance(CustomHost host, string remoteComputerName)
        {
            var powershell = createPowershell(
                host, createRemoteInfo(remoteComputerName));
            executingPipeline = createPipeline(powershell.Runspace);
            autocomplete = new Autocomplete(powershell);
        }

        public string RemoteComputerName 
            => executingPipeline.Runspace.ConnectionInfo?.ComputerName;
        public async Task<string> GetWorkingDirectory() 
            => await executingPipeline.GetWorkingDirectory();

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

        PowerShell createPowershell(
            CustomHost host, WSManConnectionInfo remoteInfo)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = createRunspace(host, remoteInfo);
            return powershell;
        }
        
        Runspace createRunspace(CustomHost host, WSManConnectionInfo remoteInfo)
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
            return pipeline;
        }
    }
}