using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Runspaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService
{
    sealed class PowerShellEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;
        public event EventHandler ExitRequested;

        readonly IShellUI ui;
        readonly LinkedList<PowerShellInstance> instances
            = new LinkedList<PowerShellInstance>();

        PowerShellInstance rootInstance => instances.First.Value;
        PowerShellInstance activeInstance => instances.Last.Value;

        public string RemoteComputerName => activeInstance.RemoteComputerName;
        public string GetWorkingDirectory()
            => activeInstance.GetWorkingDirectory();

        public PowerShellEngine(IShellUI ui)
        {
            this.ui = ui;
            instances.AddFirst(createInstance(ui, null));
        }

        PowerShellInstance createInstance(
            IShellUI ui, string remoteComputerName)
        {
            var instance = new PowerShellInstance(ui, remoteComputerName);
            instance.ExecDone += onExecDone;
            instance.ExitRequested += onExitRequested;
            return instance;
        }

        public async Task InitEnvironment()
            => await activeInstance.InitEnvironment();

        public async Task OpenRemoteSession(string computerName)
        {
            instances.AddLast(createInstance(ui, computerName));
            await activeInstance.InitEnvironment();
        }

        void onExecDone(object sender, ExecDoneEventArgs e)
        {
            // Only process events from active instance.
            if (sender != activeInstance) return;

            ExecDone(this, e);
        }

        void onExitRequested(object sender, EventArgs e)
        {
            // Only process events from active instance.
            if (sender != activeInstance) return;

            // If we are exiting the original session, let the UI layer handle
            // this.
            if (sender == rootInstance)
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
            string input,
            int index,
            bool forward)
        {
            return await activeInstance.GetCompletion(input, index, forward);
        }
    }
}