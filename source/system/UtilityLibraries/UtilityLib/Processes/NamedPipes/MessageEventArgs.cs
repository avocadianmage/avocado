using System;

namespace UtilityLib.Processes.NamedPipes
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