using System.Linq;
using System.Management.Automation.Host;

namespace AvocadoShell.PowerShellService.Host.ChoicePrompt
{
    sealed class Choice
    {
        public string HelpMessage { get; }
        public string Text { get; }
        public string Hotkey { get; }

        public Choice(ChoiceDescription desc)
        {
            Text = desc.Label.Replace("&", string.Empty);
            Hotkey = desc.Label.Split('&')[1].First().ToString().ToUpper();
            HelpMessage = desc.HelpMessage;
        }

        public override string ToString()
        {
            var str = $" [{Hotkey}] {Text}";
            return string.IsNullOrWhiteSpace(HelpMessage)
                ? str : $"{str} - {HelpMessage}";
        }
    }
}
