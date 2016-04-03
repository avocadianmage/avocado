using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using UtilityLib.Processes;

namespace AvocadoShell.PowerShellService
{
    sealed class PSEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;

        readonly IShellUI shellUI;
        readonly PowerShell ps;
        readonly Autocomplete autocomplete;
        readonly OutputFormatter formatter;
        readonly PSDataCollection<PSObject> stdOut
            = new PSDataCollection<PSObject>();

        public PSEngine(IShellUI ui)
        {
            shellUI = ui;
            ps = createPowershell(ui);
            autocomplete = new Autocomplete(ps);
            formatter = new OutputFormatter(shellUI);
        }
        
        void addStartupScriptToExec()
        {
            ps.AddScript(
                EnvUtils.GetEmbeddedText("AvocadoShell.Assets.startup.ps1"));
        }

        void addUserCmdsToExec()
        {
            var cmdArg = EnvUtils.GetArg(0);
            if (cmdArg != null) ps.AddScript(cmdArg);
        }

        public async Task InitEnvironment()
        {
            shellUI.WriteOutputLine($"Booting avocado [v{Config.Version}]");
            await doWork(
                "Starting autocompletion service",
                autocomplete.InitializeService());
            await doWork("Running startup scripts", runStartupScripts);
        }

        async Task doWork(string message, Action action)
            => await doWork(message, Task.Run(action));

        async Task doWork(string message, Task work)
        {
            shellUI.WriteCustom($"{message}...", Config.SystemFontBrush, false);
            await work;
            shellUI.WriteOutputLine("done.");
        }

        void runStartupScripts()
        {
            // Run user profile script.
            addStartupScriptToExec();

            // Execute any commands provided via commandline arguments to
            // this process.
            addUserCmdsToExec();

            // Perform the execution all at once.
            execute();
        }

        public void ExecuteCommand(string cmd)
        {
            ps.AddScript(cmd);
            execute();
        }

        void execute()
        {
            // Subscribe to command execution lifetime events.
            ps.InvocationStateChanged += onInvocationStateChanged;

            // Send the command to PowerShell to be executed.
            ps.BeginInvoke<PSObject, PSObject>(null, stdOut);
        }

        public void Stop() => ps.BeginStop(null, null);

        public async Task<string> GetCompletion(
            string input,
            int index,
            bool forward)
        {
            return await autocomplete.GetCompletion(
                input,
                index,
                forward);
        }

        PowerShell createPowershell(IShellUI ui)
        {
            var powershell = PowerShell.Create();
            powershell.Runspace = createRunspace(ui);

            // Inititalize output and error streams.
            stdOut.DataAdded += stdoutDataAdded;
            powershell.Streams.Error.DataAdded += stderrDataAdded;

            return powershell;
        }

        Runspace createRunspace(IShellUI ui)
        {
            var host = new CustomHost(ui);
            var runspace = RunspaceFactory.CreateRunspace(host);
            runspace.Open();
            return runspace;
        }

        void onInvocationStateChanged(
            object sender,
            PSInvocationStateChangedEventArgs e)
        {
            string error;
            switch (e.InvocationStateInfo.State)
            {
                case PSInvocationState.Completed:
                    error = null;
                    break;
                case PSInvocationState.Failed:
                    error = e.InvocationStateInfo.Reason.Message;
                    break;
                case PSInvocationState.Stopped:
                    error = "Execution aborted.";
                    break;
                default: return;
            }
            finishExecution(error);
        }

        void finishExecution(string error)
        {
            // Unsubscribe from command events.
            ps.InvocationStateChanged -= onInvocationStateChanged;

            // Clean up the command buffer.
            ps.Commands.Clear();

            // Fire event indicating the current execution has finished.
            ExecDone(this, new ExecDoneEventArgs(workingDirectory, error));
        }

        string workingDirectory
            => ps.Runspace.SessionStateProxy.Path.CurrentLocation.Path;

        void stderrDataAdded(object sender, DataAddedEventArgs e)
            => formatter.OutputError(ps.Streams.Error[e.Index]);

        void stdoutDataAdded(object sender, DataAddedEventArgs e)
            => formatter.OutputPSObject(stdOut[e.Index]);
    }
}