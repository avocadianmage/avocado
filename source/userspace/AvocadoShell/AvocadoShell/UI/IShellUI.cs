using System.Security;
using System.Windows.Media;

namespace AvocadoShell.UI
{
    interface IShellUI
    {
        void WriteOutputLine(string text);
        void WriteErrorLine(string text);
        void WriteCustom(string text, Brush foreground, bool newline);
        string WritePrompt(string prompt);
        SecureString WriteSecurePrompt(string prompt);
    }
}