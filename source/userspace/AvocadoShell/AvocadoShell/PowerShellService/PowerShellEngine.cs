using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Modules;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService
{
    sealed class PowerShellEngine
    {
        public CustomHost MyHost { get; private set; }
        public History MyHistory { get; private set; }

        public string RemoteComputerName
            => MyHost.Runspace.ConnectionInfo?.ComputerName;
        public string GetWorkingDirectory() => getPSVariable("PWD").ToString();

        public string ExecuteCommand(string command)
        {
            MyHistory.Add(command);
            return MyHost.CurrentPipeline.ExecuteCommand(command);
        }

        public void Initialize(IShellUI shellUI)
        {
            MyHost = new CustomHost(shellUI);
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