using System;

namespace AvocadoShell.PowerShellService
{
    sealed class ExecDoneEventArgs : EventArgs
    {
        public string Path { get { return path; } }
        public string Error { get { return error; } }

        readonly string path;
        readonly string error;

        public ExecDoneEventArgs(string path, string error)
        {
            this.path = path;
            this.error = error;
        }
    }
}