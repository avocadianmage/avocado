using System;

namespace StandardLibrary.Processes.NamedPipes
{
    public sealed class MessageEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}