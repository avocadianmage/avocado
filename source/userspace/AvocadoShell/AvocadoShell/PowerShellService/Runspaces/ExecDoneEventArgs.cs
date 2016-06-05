using System;

namespace AvocadoShell.PowerShellService.Runspaces
{
    sealed class ExecDoneEventArgs : EventArgs
    {
        public string Error { get; }

        public ExecDoneEventArgs() : this(null) { }

        public ExecDoneEventArgs(string error)
        {
            Error = error;
        }
    }
}