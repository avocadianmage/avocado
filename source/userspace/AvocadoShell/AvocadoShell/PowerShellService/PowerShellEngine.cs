using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Modules;
using AvocadoShell.Terminal;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService
{
    sealed class PowerShellEngine
    {
        public CustomHost MyHost { get; }
        public History MyHistory { get; private set; }

        public string RemoteComputerName
            => MyHost.Runspace.ConnectionInfo?.ComputerName;
        public string GetWorkingDirectory() => getPSVariable("PWD").ToString();

        public string ExecuteCommand(string command)
        {
            MyHistory.Add(command);
            return MyHost.CurrentPipeline.ExecuteCommand(command);
        }

        public bool Stop() => MyHost.CurrentPipeline.Stop();

        public bool GetCompletion(ref string input, ref int index, bool forward)
            => MyHost.CurrentPipeline.Autocomplete.GetCompletion(
                ref input, ref index, forward);

        public PowerShellEngine(IShellUI shellUI)
        {
            MyHost = new CustomHost(shellUI);
        }

        public void Initialize()
        {
            createInitialHostRunspace();
            MyHistory = createHistory();
        }

        void createInitialHostRunspace()
        {
            var runspace = RunspaceFactory.CreateRunspace(MyHost);
            runspace.Open();
            MyHost.PushRunspace(runspace);
        }

        History createHistory()
        {
            var maxHistoryCount = (int)getPSVariable("MaximumHistoryCount");
            return new History(maxHistoryCount);
        }

        object getPSVariable(string name)
            => MyHost.Runspace.SessionStateProxy.GetVariable(name);
    }
}