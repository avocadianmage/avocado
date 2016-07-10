﻿using System.Security;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AvocadoShell.Terminal
{
    interface IShellUI
    {
        void WriteOutputLine(string data);
        void WriteErrorLine(string data);
        void WriteCustom(string data, Brush foreground, bool newline);

        string WritePrompt(string prompt);
        SecureString WriteSecurePrompt(string prompt);

        Task RunNativeCommand(string message);
    }
}