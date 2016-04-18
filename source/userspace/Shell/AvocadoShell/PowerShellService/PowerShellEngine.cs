using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Runspaces;
using System;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService
{
    sealed class PowerShellEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;
        public event EventHandler ExitRequested;

        readonly IShellUI shellUI;
        readonly PowerShellInstance localInstance;

        PowerShellInstance currentInstance;

        public PowerShellEngine(IShellUI ui)
        {
            shellUI = ui;
            localInstance = new PowerShellInstance(ui);
            updateCurrentInstance(localInstance);
        }

        public async Task InitEnvironment()
            => await localInstance.InitEnvironment();

        public async Task OpenRemoteSession(string computerName)
        {
            // Unsubscribe from the events of the local PowerShell instance.
            localInstance.ExecDone -= onExecDone;
            localInstance.ExitRequested -= onExitRequested;

            // Create the new remote PowerShell instance.
            var remoteInstance = new PowerShellInstance(shellUI, computerName);
            updateCurrentInstance(remoteInstance);
            await currentInstance.InitEnvironment();
        }

        void updateCurrentInstance(PowerShellInstance instance)
        {
            instance.ExecDone += onExecDone;
            instance.ExitRequested += onExitRequested;
            currentInstance = instance;
        }

        void onExecDone(object sender, ExecDoneEventArgs e)
            => ExecDone(this, e);

        void onExitRequested(object sender, EventArgs e)
            => ExitRequested(this, e);

        public void ExecuteCommand(string cmd)
            => localInstance.ExecuteCommand(cmd);

        public void Stop() => localInstance.Stop();

        public async Task<string> GetCompletion(
            string input,
            int index,
            bool forward)
        {
            return await localInstance.GetCompletion(input, index, forward);
        }
    }
}