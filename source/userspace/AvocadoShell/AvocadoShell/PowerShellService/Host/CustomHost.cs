using AvocadoShell.PowerShellService.Execution;
using AvocadoShell.PowerShellService.Host.UI;
using AvocadoShell.Terminal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

namespace AvocadoShell.PowerShellService.Host
{
    sealed class CustomHost : PSHost, IHostSupportsInteractiveSession
    {
        public bool ShouldExit { get; private set; }
        public string RemoteComputerName
            => Runspace.ConnectionInfo?.ComputerName;
        public string GetWorkingDirectory() => getPSVariable("PWD").ToString();

        RunspacePipeline currentPipeline => pipelines.Peek();

        readonly IShellUI shellUI;
        readonly Stack<RunspacePipeline> pipelines
            = new Stack<RunspacePipeline>();
        History history;

        public CustomHost(IShellUI shellUI)
        {
            this.shellUI = shellUI;
            UI = new CustomHostUI(shellUI);
        }

        public void InitializeEnvironment()
        {
            // Initialize runspace.
            var runspace = RunspaceFactory.CreateRunspace(this);
            runspace.Open();
            PushRunspace(runspace);

            // Initialize history.
            history = createHistory();
        }

        History createHistory()
        {
            var maxHistoryCount = (int)getPSVariable("MaximumHistoryCount");
            return new History(maxHistoryCount);
        }

        object getPSVariable(string name)
            => Runspace.SessionStateProxy.GetVariable(name);

        public string ExecuteCommand(string command)
        {
            history.Add(command);
            return currentPipeline.ExecuteCommand(command);
        }

        public bool Stop() => currentPipeline.Stop();

        public bool GetCompletion(ref string input, ref int index, bool forward)
            => currentPipeline.Autocomplete.GetCompletion(
                ref input, ref index, forward);

        public string CycleHistory(string currentInput, bool forward)
            => history.Cycle(currentInput, forward);

        /// <summary>
        /// Gets the culture information to use. This implementation returns a
        /// snapshot of the culture information of the thread that created this
        /// object.
        /// </summary>
        public override CultureInfo CurrentCulture { get; }
            = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Gets the UI culture information to use. This implementation returns
        /// a snapshot of the UI culture information of the thread that created
        /// this object.
        /// </summary>
        public override CultureInfo CurrentUICulture { get; }
            = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Gets an identifier for this host. This implementation always returns
        /// the GUID allocated at instantiation time.
        /// </summary>
        public override Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets a string that contains the name of this host implementation. 
        /// Keep in mind that this string may be used by script writers to
        /// identify when your host is being used.
        /// </summary>
        public override string Name => "avocado";

        /// <summary>
        /// Gets an instance of the implementation of the PSHostUserInterface
        /// class for this application. This instance is allocated once at 
        /// startup time and returned every time thereafter.
        /// </summary>
        public override PSHostUserInterface UI { get; }

        /// <summary>
        /// Gets the version object for this application. Typically this 
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version
            => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Instructs the host to interrupt the currently running pipeline and
        /// start a new nested input loop.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Instructs the host to exit the currently running input loop.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is called before an external application process is started.
        /// Typically it is used to save state so that the parent can restore
        /// state that has been modified by a child process (after the child 
        /// exits).
        /// </summary>
        public override void NotifyBeginApplication()
        {
            // Empty implementation.
        }

        /// <summary>
        /// This is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered.
        /// </summary>
        public override void NotifyEndApplication()
        {
            // Empty implementation.
        }

        /// <summary>
        /// Indicate to the host application that exit has been requested. Pass
        /// the exit code that the host application should use when exiting the 
        /// process.
        /// </summary>
        /// <param name="exitCode">The exit code that the host application
        /// should use.</param>
        public override void SetShouldExit(int exitCode)
        {
            if (IsRunspacePushed) PopRunspace();
            else ShouldExit = true;
        }

        #region IHostSupportsInteractiveSession implementation.

        /// <summary>
        /// Gets a value indicating whether a request to open a PSSession has
        /// been made.
        /// </summary>
        public bool IsRunspacePushed => pipelines.Count > 1;

        /// <summary>
        /// Gets or sets the runspace used by the PSSession.
        /// </summary>
        public Runspace Runspace => currentPipeline.Runspace;

        /// <summary>
        /// Requests to close a PSSession.
        /// </summary>
        public void PopRunspace() => pipelines.Pop();

        /// <summary>
        /// Requests to open a PSSession.
        /// </summary>
        /// <param name="runspace">Runspace to use.</param>
        public void PushRunspace(Runspace runspace)
        {
            var pipeline = new RunspacePipeline(runspace);
            pipelines.Push(pipeline);

            // Initialize the PowerShell environment of the pipeline.
            var error = pipeline.InitEnvironment();
            if (error != null) shellUI.WriteErrorLine(error);
        }

        #endregion
    }
}
