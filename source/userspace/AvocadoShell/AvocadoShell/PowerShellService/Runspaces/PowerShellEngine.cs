using AvocadoShell.PowerShellService.Host;
using AvocadoShell.Terminal;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class PowerShellEngine
    {
        public bool ShouldExit => host.ShouldExit;
        public string RemoteComputerName
            => host.Runspace.ConnectionInfo?.ComputerName;
        public string GetWorkingDirectory() => getPSVariable("PWD").ToString();
        public int GetMaxHistoryCount()
            => (int)getPSVariable("MaximumHistoryCount");

        object getPSVariable(string name)
            => host.Runspace.SessionStateProxy.GetVariable(name);

        readonly CustomHost host;

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

        public string ExecuteCommand(string cmd)
            => host.Pipeline.ExecuteCommand(cmd);

        public bool Stop() => host.Pipeline.Stop();

        public bool GetCompletion(ref string input, ref int index, bool forward)
            => host.Pipeline.Autocomplete.GetCompletion(
                ref input, ref index, forward);

        public void SetHostBufferSize(int width, int height)
            => host.UI.RawUI.BufferSize = new Size(width, height);
    }
}