using AvocadoShell.PowerShellService.Host;
using AvocadoShell.Terminal;
using System;
using System.Collections.Generic;
using System.Management.Automation.Host;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class PowerShellEngine
    {
        public event EventHandler ExitRequested;
        
        readonly CustomHost psHost;
        readonly LinkedList<PowerShellInstance> instances
            = new LinkedList<PowerShellInstance>();

        PowerShellInstance localInstance => instances.First.Value;
        PowerShellInstance activeInstance => instances.Last.Value;

        public PSHostRawUserInterface HostRawUI => psHost.UI.RawUI;
        public string RemoteComputerName => activeInstance.RemoteComputerName;
        public string GetWorkingDirectory()
            => activeInstance.GetWorkingDirectory();
        public int GetMaxHistoryCount() => activeInstance.GetMaxHistoryCount();

        public PowerShellEngine(IShellUI ui)
        {
            psHost = createHost(ui);
            instances.AddFirst(createInstance(null));
        }

        CustomHost createHost(IShellUI ui)
        {
            var host = new CustomHost(ui);
            host.ExitRequested += onExitRequested;
            return host;
        }

        PowerShellInstance createInstance(string remoteComputerName)
            => new PowerShellInstance(psHost, remoteComputerName);

        public void InitEnvironment() => activeInstance.InitEnvironment();
        
        public void OpenRemoteSession(string computerName)
        {
            instances.AddLast(createInstance(computerName));
            activeInstance.InitEnvironment();
        }
        
        public void DownloadRemote(string paths)
        {
            localInstance.ExecuteCommand(
                $"SendToLocal {activeInstance.RemoteComputerName} {paths}");
        }

        void onExitRequested(object sender, EventArgs e)
        {
            // If we are exiting the original session, let the UI layer handle
            // this.
            if (activeInstance == localInstance)
            {
                ExitRequested(this, e);
                return;
            }

            // Pop the active instance.
            instances.RemoveLast();
        }

        public void ExecuteCommand(string cmd)
            => activeInstance.ExecuteCommand(cmd);

        public bool Stop() => activeInstance.Stop();

        public string GetCompletion(string input, int index, bool forward)
            => activeInstance.GetCompletion(input, index, forward);
    }
}