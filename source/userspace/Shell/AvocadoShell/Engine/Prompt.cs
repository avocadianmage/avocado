using AvocadoUtilities;
using UtilityLib.Processes;

namespace AvocadoShell.Engine
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
            // Replace the root directory string with the tilde alias.
            if (path.StartsWith(RootDir.Val))
            {
                path = path.Replace(RootDir.Val, "~");
            }

            // Indicate if this shell has administrative permissions.
            var adminStr = EnvUtils.IsAdmin ? "[admin] " : string.Empty;

            // Return the formatted prompt string.
            return $"{adminStr}{path} ";
        }
    }
}