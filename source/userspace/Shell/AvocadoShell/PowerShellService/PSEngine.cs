using AvocadoShell.Engine;
using AvocadoShell.PowerShellService.Host;
using AvocadoUtilities;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService
{
    sealed class PSEngine
    {
        public event EventHandler<ExecDoneEventArgs> ExecDone;

        readonly PowerShell ps;
        readonly IShellUI shellUI;
        readonly Autocomplete autocomplete;
        readonly PSDataCollection<PSObject> stdOut
            = new PSDataCollection<PSObject>();

        public PSEngine(IShellUI shellUI)
        {
            ps = PowerShell.Create();
            this.shellUI = shellUI;
            autocomplete = new Autocomplete(ps);

            preparePowershell();
            initStandardOutput();
            initStandardError();
        }

        static string getIncludeScript(string dir, string script)
        {
            const string FMT = ". {0}";
            var path = Path.Combine(dir, script);
            return string.Format(FMT, path);
        }

        void addProfileScriptToExec()
        {
            var dir = RootDir.Avocado.Apps.MyAppPath;
            var script = getIncludeScript(dir, "profile.ps1");
            ps.AddScript(script);
        }

        void addUserCmdsToExec()
        {
            var cmdArg = AvocadoUtilities.Command.GetArg(1);
            if (cmdArg != null) ps.AddScript(cmdArg);
        }

        public void InitEnvironment()
        {
            // Run user profile script.
            addProfileScriptToExec();

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
            invocationStateSubscribe();
            ps.BeginInvoke<PSObject, PSObject>(null, stdOut);
        }

        public void Stop()
        {
            ps.BeginStop(null, null);
        }

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
            string error;
            switch (e.InvocationStateInfo.State)
            {
                case PSInvocationState.Failed:
                    error = e.InvocationStateInfo.Reason.Message;
                    break;
                case PSInvocationState.Completed:
                    error = null;
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
            invocationStateUnsubscribe();
            ps.Commands.Clear();

            // Fire event indicating the current execution has finished.
            var path = ps.Runspace.SessionStateProxy
                .Path.CurrentLocation.Path;
            ExecDone(this, new ExecDoneEventArgs(path, error));
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