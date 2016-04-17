using UtilityLib.Processes;

namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell { get; }
        public int LinePos { get; }
    
        public Prompt(bool fromShell, int linePos)
        {
            FromShell = fromShell;
            LinePos = linePos;
        }

        public static string GetShellPromptStr(string path)
        {
            var promptStr = $"{path} ";

            // Indicate if this shell has administrative permissions.
            if (EnvUtils.IsAdmin) promptStr = $"[root] {promptStr}";

            return promptStr;
        }
    }
}