using System.Windows.Media;

namespace AvocadoShell.Engine
{
    interface IShellUI
    {
        void RequestExit();

        void WriteOutputLine(string data);
        void WriteErrorLine(string data);
        void WriteCustom(string data, Brush foreground, bool newline);

        string ReadLine();
    }
}