using System;

namespace AvocadoShell.PowerShellService
{
    sealed class ExecDoneEventArgs : EventArgs
    {
        public string Path { get; }
        public string Error { get; }

        public ExecDoneEventArgs(string path, string error)
        {
            Path = path;
            Error = error;
        }
    }
}