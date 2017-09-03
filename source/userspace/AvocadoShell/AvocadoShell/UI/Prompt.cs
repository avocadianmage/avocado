using System;
using System.Text;

namespace AvocadoShell.UI
{
    sealed class Prompt
    {
        public event EventHandler<string> PromptUpdated;

        public Prompt()
        {
            var notifyingDateTime = new NotifyingDateTime();
            notifyingDateTime.PropertyChanged += (s, e) =>
            {
                var promptText = notifyingDateTime.Now.ToString(Format);
                PromptUpdated?.Invoke(s, promptText);
            };
        }

        public static string GetShellTitleString(
            string workingDirectory, string remoteComputerName)
        {
            var sb = new StringBuilder();
            if (remoteComputerName != null)
                sb.Append($"[{remoteComputerName}] ");
            return sb.Append(workingDirectory).ToString();
        }

        public string Format => "yyyy.MM.dd-HH:mm:ss ";
        public bool FromShell { get; set; }
        public string ShellTitle { get; set; }
        public int StartOffset { get; set; }
    }
}