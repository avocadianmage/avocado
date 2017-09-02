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
            return sb.Append(workingDirectory).ToString();
        }

        public bool FromShell { get; set; }
        public int EndOffset { get; set; }
        public string ShellTitle { get; set; }

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
        Run _shellTimestampRun;

        readonly Binding bindableDateTime = new Binding
        {
            Source = new NotifyingDateTime(),
            Path = new PropertyPath(nameof(NotifyingDateTime.Now)),
            Mode = BindingMode.OneWay,
            StringFormat = "yyyy.MM.dd-HH:mm:ss "
        };
    }
}