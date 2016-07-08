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

        readonly Runspace runspace;
        readonly ExecutingPipeline executingPipeline;
        readonly Autocomplete autocomplete;

        public PowerShellInstance(CustomHost host) : this(host, null) { }

        public PowerShellInstance(CustomHost host, string remoteComputerName)
        {
            var remoteInfo = createRemoteInfo(remoteComputerName);
            runspace = createRunspace(host, remoteInfo);
            var powershell = createPowershell(runspace);
            executingPipeline = createPipeline(runspace);
            autocomplete = new Autocomplete(powershell);
        }

        public string RemoteComputerName 
            => runspace.ConnectionInfo?.ComputerName;

        public async Task<IEnumerable<string>> RunBackgroundCommand(
            string command)
        {
            var result = await Task.Run(
                () => runspace.CreatePipeline(command).Invoke());
            return result.Select(l => l.ToString());
        }

        public async Task<string> GetWorkingDirectory()
        {
            var path = await getPSVariable("PWD");
            var homeDir = Environment.GetFolderPath(
               Environment.SpecialFolder.UserProfile);
            return path.ToString().Replace(homeDir, "~");
        }

        async Task<object> getPSVariable(string name)
        {
            return await Task.Run(
                () => runspace.SessionStateProxy.GetVariable(name));
        }

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

        PowerShell createPowershell(Runspace runspace)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = runspace;
            return powershell;
        }
        
        Runspace createRunspace(CustomHost host, WSManConnectionInfo remoteInfo)
        {
            var rs = remoteInfo == null
                ? RunspaceFactory.CreateRunspace(host)
                : RunspaceFactory.CreateRunspace(
                    remoteInfo, host, TypeTable.LoadDefaultTypeFiles());
            rs.Open();
            return rs;
        }

        ExecutingPipeline createPipeline(Runspace runspace)
        {
            var pipeline = new ExecutingPipeline(runspace);
            pipeline.Done += (s, e) => ExecDone(this, e);
            return pipeline;
        }
    }
}