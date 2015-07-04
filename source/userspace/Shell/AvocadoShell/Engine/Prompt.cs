using AvocadoUtilities;
using UtilityLib.MiscTools;

namespace AvocadoShell.Engine
{
    sealed class Prompt
    {
        public bool FromShell { get { return fromShell; } }
        public string Text { get { return text; } }

        readonly bool fromShell;
        readonly string text;

        public static Prompt CreateShellPrompt(string path)
        {
            var str = getShellPrompt(path);
            return new Prompt(true, str);
        }

        public static Prompt CreateOtherPrompt(string text)
        {
            return new Prompt(false, text);
        }

        Prompt(bool fromShell, string text)
        {
            this.fromShell = fromShell;
            this.text = text;
        }

        static string getShellPrompt(string path)
        {
            // Replace the root directory string with the tilde alias.
            if (path.StartsWith(RootDir.Val))
            {
                path = path.Replace(RootDir.Val, "~");
            }

            // Indicate if this shell has administrative permissions.
            var adminStr = Tools.IsAdmin ? "[admin] " : string.Empty;

            // Return the formatted prompt string.
            return string.Format("{0}{1} ", adminStr, path);
        }
    }
}