using StandardLibrary.Processes;
using System;
using System.Text;

namespace AvocadoShell.UI
{
    sealed class Prompt
    {
        public static string GetShellTitleString(
            string workingDirectory, string remoteComputerName)
        {
            var sb = new StringBuilder();
            if (remoteComputerName != null)
                sb.Append($"[{remoteComputerName}] ");
            return sb.Append(workingDirectory).ToString();
        }

        public static string GetShellPromptString
            => $"{(EnvUtils.IsAdmin ? "root" : Environment.UserName)}~$ ";

        public bool FromShell { get; set; }
        public string ShellTitle { get; set; }
    }
}