using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoShell.PowerShellService.Runspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService
{
    sealed class PowerShellEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;
        public event EventHandler ExitRequested;
        
        readonly CustomHost psHost;
        readonly LinkedList<PowerShellInstance> instances
            = new LinkedList<PowerShellInstance>();

        PowerShellInstance localInstance => instances.First.Value;
        PowerShellInstance activeInstance => instances.Last.Value;

        public PSHostRawUserInterface HostRawUI => psHost.UI.RawUI;
        public string RemoteComputerName => activeInstance.RemoteComputerName;
        public async Task<string> GetWorkingDirectory()
            => await activeInstance.GetWorkingDirectory();

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
        {
            var instance = new PowerShellInstance(psHost, remoteComputerName);
            instance.ExecDone += onExecDone;
            return instance;
        }

        public void InitEnvironment() => activeInstance.InitEnvironment();

        public async Task<bool> RunNativeCommand(string message)
        {
            const string AVOCADO_PREFX = "avocado:";
            if (!message.StartsWith(AVOCADO_PREFX)) return false;

            var pieces = message.Substring(AVOCADO_PREFX.Length).Split(' ');
            var arg = string.Join(" ", pieces.Skip(1));
            switch (pieces.First())
            {
                case "Enter-PSSession":
                    openRemoteSession(arg);
                    break;
                case "Download-Remote":
                    await downloadRemote(arg);
                    break;
            }
            return true;
        }

        void openRemoteSession(string computerName)
        {
            instances.AddLast(createInstance(computerName));
            activeInstance.InitEnvironment();
        }
        
        async Task downloadRemote(string paths)
        {
            var computerName = activeInstance.RemoteComputerName;
            await localInstance.RunBackgroundCommand(
                $"SendToLocal {computerName} {paths}");
        }

        void onExecDone(object sender, ExecDoneEventArgs e)
        {
            // Only process events from active instance.
            if (sender != activeInstance) return;

            ExecDone(this, e);
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

            // Retrigger local prompt.
            ExecDone(this, new ExecDoneEventArgs());
        }

        public void ExecuteCommand(string cmd)
            => activeInstance.ExecuteCommand(cmd);

        public void Stop() => activeInstance.Stop();

        public async Task<string> GetCompletion(
            string input, int index, bool forward)
            => await activeInstance.GetCompletion(input, index, forward);
    }
}