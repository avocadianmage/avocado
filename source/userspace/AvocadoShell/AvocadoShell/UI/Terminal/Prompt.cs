using StandardLibrary.Processes;
using System;
using System.Text;

namespace AvocadoShell.UI.Terminal
{
    sealed class Prompt
    {
        public bool FromShell { get; private set; }
        public int LengthInSymbols { get; private set; }
        public string ShellTitle { get; set; }

        public void Update(bool fromShell, int lengthInSymbols)
        {
            FromShell = fromShell;
            LengthInSymbols = lengthInSymbols;
        }

        public static string ElevatedPrefix
            => EnvUtils.IsAdmin ? "root " : string.Empty;

        public static string GetShellTitleString(
            string workingDirectory, string remoteComputerName)
        {
            var sb = new StringBuilder();
            if (remoteComputerName != null)
                sb.Append($"[{remoteComputerName}] ");
            sb.Append(formatPathForDisplay(workingDirectory));
            return sb.ToString();
        }

        static string formatPathForDisplay(string path)
        {
            var homeDir = Environment.GetFolderPath(
               Environment.SpecialFolder.UserProfile);
            return path.Replace(homeDir, "~");
        }
    }
}