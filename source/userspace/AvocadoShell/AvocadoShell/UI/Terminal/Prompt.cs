using System;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace AvocadoShell.UI.Terminal
{
    sealed class Prompt
    {
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

        public bool FromShell { get; set; }
        public int LengthInSymbols { get; set; }
        public string ShellTitle { get; set; }

        Run _shellTimestampRun;
        public Run ShellTimestampRun
        {
            get { return _shellTimestampRun; }
            set
            {
                if (_shellTimestampRun != null)
                {
                    BindingOperations.ClearBinding(
                        _shellTimestampRun, Run.TextProperty);
                    var notifyingDateTime 
                        = (NotifyingDateTime)bindableDateTime.Source;
                    _shellTimestampRun.Text = notifyingDateTime.Now.ToString(
                        bindableDateTime.StringFormat);
                }

                BindingOperations.SetBinding(
                    value, Run.TextProperty, bindableDateTime);
                _shellTimestampRun = value;
            }
        }

        readonly Binding bindableDateTime = new Binding
        {
            Source = new NotifyingDateTime(),
            Path = new PropertyPath(nameof(NotifyingDateTime.Now)),
            Mode = BindingMode.OneWay,
            StringFormat = "MM.dd.yyyy-HH:mm:ss>"
        };
    }
}