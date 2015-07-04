using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoUtilities;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService
{
    sealed class PSEngine
    {
        public Autocomplete Autocompleter { get { return autocomplete; } }

        readonly PowerShell ps;
        readonly IShellUI shellUI;
        readonly Autocomplete autocomplete;
        readonly PSDataCollection<PSObject> stdOut
            = new PSDataCollection<PSObject>();

        bool aborted = false;

        public PSEngine(IShellUI shellUI)
        {
            ps = PowerShell.Create();
            this.shellUI = shellUI;
            autocomplete = new Autocomplete(ps);

            preparePowershell();
            initStandardOutput();
            initStandardError();
        }

        public void AddScript(string input)
        {
            ps.AddScript(input);
        }

        public void Execute()
        {
            invocationStateSubscribe();
            ps.BeginInvoke<PSObject, PSObject>(null, stdOut);
        }

        public void Stop()
        {
            ps.BeginStop(null, null);
        }

        void preparePowershell()
        {
            var host = new CustomHost(shellUI);
            var session = createSession();
            var runspace = RunspaceFactory.CreateRunspace(host, session);
            runspace.Open();
            ps.Runspace = runspace;
        }

        InitialSessionState createSession()
        {
            var cmdlets = Directory.EnumerateFiles(
                RootDir.Avocado.Apps.Val, 
                "*.dll", 
                SearchOption.AllDirectories);
            var state = InitialSessionState.CreateDefault();
            state.ImportPSModule(cmdlets.ToArray());
            return state;
        }

        void invocationStateSubscribe()
        {
            ps.InvocationStateChanged += onInvocationStateChanged;
        }

        void invocationStateUnsubscribe()
        {
            ps.InvocationStateChanged -= onInvocationStateChanged;
        }

        void onInvocationStateChanged(
            object sender,
            PSInvocationStateChangedEventArgs e)
        {
            switch (e.InvocationStateInfo.State)
            {
                case PSInvocationState.Stopping:
                    aborted = true;
                    break;
                case PSInvocationState.Failed:
                    var reason = e.InvocationStateInfo.Reason.Message;
                    executionFail(reason);
                    break;
                case PSInvocationState.Completed:
                case PSInvocationState.Stopped:
                    finishExecution();
                    break;
            }
        }

        void executionFail(string reason)
        {
            shellUI.WriteErrorLine(reason);
            finishExecution();
        }

        void finishExecution()
        {
            invocationStateUnsubscribe();
            ps.Commands.Clear();

            // If the execution was aborted, inform the user.
            if (aborted)
            {
                aborted = false;
                shellUI.WriteErrorLine("Execution aborted.");
            }

            var path = ps.Runspace.SessionStateProxy
                .Path.CurrentLocation.Path;
            shellUI.DisplayShellPrompt(path);
        }

        void initStandardError()
        {
            ps.Streams.Error.DataAdded += stderrDataAdded;
        }

        void initStandardOutput()
        {
            stdOut.DataAdded += stdoutDataAdded;
        }

        void stderrDataAdded(object sender, DataAddedEventArgs e)
        {
            var data = ps.Streams.Error[e.Index].ToString();
            shellUI.WriteErrorLine(data);
        }

        void stdoutDataAdded(object sender, DataAddedEventArgs e)
        {
            var data = stdOut[e.Index].ToString();
            shellUI.WriteSystemLine(data);
            var str = ps.HistoryString;
        }
    }
}