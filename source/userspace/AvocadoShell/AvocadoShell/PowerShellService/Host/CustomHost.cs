﻿using AvocadoShell.Terminal;
using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;
using System.Management.Automation;

namespace AvocadoShell.PowerShellService.Host
{
    /// <summary>
    /// This is a sample implementation of the PSHost abstract class for 
    /// console applications. Not all members are implemented. Those that 
    /// are not implemented throw a NotImplementedException exception or 
    /// return nothing.
    /// </summary>
    class CustomHost : PSHost
    {
        public event EventHandler ExitRequested;

        readonly IShellUI shellUI;

        public CustomHost(IShellUI shellUI)
        {
            this.shellUI = shellUI;
            UI = new CustomHostUI(shellUI);
        }
        
        // Stores CustomHost object for invoking native commands.
        public override PSObject PrivateData => new PSObject(this);
        
        // Native command for interactive remoting.
        public void OpenRemoteSession(string computerName, PSCredential cred)
            => shellUI.OpenRemoteSession(computerName, cred).Wait();

        /// <summary>
        /// Gets the culture information to use. This implementation 
        /// returns a snapshot of the culture information of the thread 
        /// that created this object.
        /// </summary>
        public override CultureInfo CurrentCulture { get; }
            = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Gets the UI culture information to use. This implementation 
        /// returns a snapshot of the UI culture information of the thread 
        /// that created this object.
        /// </summary>
        public override CultureInfo CurrentUICulture { get; }
            = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Gets an identifier for this host. This implementation always 
        /// returns the GUID allocated at instantiation time.
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
        /// class for this application. This instance is allocated once at startup time
        /// and returned every time thereafter.
        /// </summary>
        public override PSHostUserInterface UI { get; }

        /// <summary>
        /// Gets the version object for this application. Typically this 
        /// should match the version resource in the application.
        /// </summary>
        public override Version Version => Config.Version;

        /// <summary>
        /// This API Instructs the host to interrupt the currently running 
        /// pipeline and start a new nested input loop. In this example this 
        /// functionality is not needed so the method throws a 
        /// NotImplementedException exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API instructs the host to exit the currently running input loop. 
        /// In this example this functionality is not needed so the method 
        /// throws a NotImplementedException exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API is called before an external application process is 
        /// started. Typically it is used to save state so that the parent  
        /// can restore state that has been modified by a child process (after 
        /// the child exits). In this example this functionality is not  
        /// needed so the method returns nothing.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            // Empty implementation.
        }

        /// <summary>
        /// This API is called after an external application process finishes.
        /// Typically it is used to restore state that a child process has
        /// altered. In this example, this functionality is not needed so  
        /// the method returns nothing.
        /// </summary>
        public override void NotifyEndApplication()
        {
            // Empty implementation.
        }


        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the 
        /// host application should use.</param>
        public override void SetShouldExit(int exitCode)
            => ExitRequested(this, new EventArgs());
    }
}
