using AvocadoShell.PowerShellService.Host;
using AvocadoShell.Terminal;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class PowerShellEngine
    {
        public bool ShouldExit => host.ShouldExit;

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
            createRunspace(host);
        }

        void createRunspace(CustomHost host)
        {
            var runspace = RunspaceFactory.CreateRunspace(host);
            runspace.Open();
            host.PushRunspace(runspace);
        }

        public string InitEnvironment() => host.Pipeline.InitEnvironment();

        public string ExecuteCommand(string cmd)
            => host.Pipeline.ExecuteCommand(cmd);

        public bool Stop() => host.Pipeline.Stop();

        public bool GetCompletion(ref string input, ref int index, bool forward)
            => host.Pipeline.Autocomplete.GetCompletion(
                ref input, ref index, forward);
    }
}