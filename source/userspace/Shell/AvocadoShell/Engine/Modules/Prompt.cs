using System;
using UtilityLib.Processes;

namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell => fromShell;
        public int LinePos => linePos;

        readonly bool fromShell;
        readonly int linePos;

        public Prompt(bool fromShell, int linePos)
        {
            this.fromShell = fromShell;
            this.linePos = linePos;
        }

        public static string GetShellPromptStr(string path)
        {
            var homeDir = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

            // Replace the root directory string with the tilde alias.
            if (path.StartsWith(homeDir)) path = path.Replace(homeDir, "~");

            var promptStr = $"{path} ";

            // Indicate if this shell has administrative permissions.
            if (EnvUtils.IsAdmin) promptStr = $"[admin] {promptStr}";

            return promptStr;
        }
    }
}