using AvocadoUtilities;
using System.Windows.Documents;
using UtilityLib.Processes;

namespace AvocadoShell.Engine
{
    sealed class Prompt
    {
        public bool FromShell => fromShell;
        public int LinePos => linePos;
        public TextPointer Pointer => pointer;

        readonly bool fromShell;
        readonly int linePos;
        readonly TextPointer pointer;

        public Prompt(bool fromShell, int linePos, TextPointer pointer)
        {
            this.fromShell = fromShell;
            this.linePos = linePos;
            this.pointer = pointer;
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
            return string.Format("{0}{1} ", adminStr, path);
        }
    }
}