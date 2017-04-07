using System.Linq;
using System.Management.Automation.Host;

namespace AvocadoShell.PowerShellService.Host.ChoicePrompt
{
    sealed class Choice
    {
        public string HelpMessage { get; }
        public string Text { get; }
        public string Hotkey { get; }

        public Choice(ChoiceDescription desc) : this(
            desc.Label, 
            desc.Label.Split('&')[1].First().ToString().ToUpper(),
            desc.HelpMessage)
        { }

        public Choice(string text, string hotkey, string helpMessage)
        {
            Text = text.Replace("&", string.Empty);
            Hotkey = hotkey;
            HelpMessage = helpMessage;
        }

        public override string ToString()
        {
            var str = $" [{Hotkey}] {Text}";
            return string.IsNullOrWhiteSpace(HelpMessage)
                ? str : $"{str} - {HelpMessage}";
        }
    }
}
