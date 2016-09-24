using AvocadoShell.PowerShellService.Host;
using AvocadoShell.Terminal;
using System;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class PowerShellEngine
    {
        public event EventHandler ExitRequested;
        
        readonly CustomHost host;

        public PSHostRawUserInterface HostRawUI => host.UI.RawUI;
        public string RemoteComputerName
            => host.Runspace.ConnectionInfo?.ComputerName;
        public string GetWorkingDirectory() => getPSVariable("PWD").ToString();
        public int GetMaxHistoryCount()
            => (int)getPSVariable("MaximumHistoryCount");

        object getPSVariable(string name)
            => host.Runspace.SessionStateProxy.GetVariable(name);

        public PowerShellEngine(IShellUI ui)
        {
            host = new CustomHost(ui);
            host.ExitRequested += onExitRequested;
            createRunspace(host);
        }

        void createRunspace(CustomHost host)
        {
            var runspace = RunspaceFactory.CreateRunspace(host);
            runspace.Open();
            host.PushRunspace(runspace);
        }

        void onExitRequested(object sender, EventArgs e)
        {
            if (host.IsRunspacePushed) host.PopRunspace();
            else ExitRequested(this, EventArgs.Empty);
        }

        public string ExecuteCommand(string cmd)
            => host.Pipeline.ExecuteCommand(cmd);

        public bool Stop() => host.Pipeline.Stop();

        public bool GetCompletion(ref string input, ref int index, bool forward)
            => host.Pipeline.Autocomplete.GetCompletion(
                ref input, ref index, forward);
    }
}