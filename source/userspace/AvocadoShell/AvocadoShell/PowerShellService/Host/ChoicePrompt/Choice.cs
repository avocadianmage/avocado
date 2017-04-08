using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Host;

namespace AvocadoShell.PowerShellService.Host.ChoicePrompt
{
    sealed class Choice
    {
        public string HelpMessage { get; }
        public string Text { get; }
        public string Hotkey { get; }

        public Choice(string text, string helpMessage) : this(
            text,
            text.Split('&')[1].First().ToString().ToUpper(),
            helpMessage)
        { }

        public Choice(string text, string hotkey, string helpMessage)
        {
            Text = text.Replace("&", string.Empty);
            Hotkey = hotkey;
            HelpMessage = helpMessage;
        }

        public string ToDisplayLine(int hotkeyTotalPadding)
        {
            var str = $" [{Hotkey.PadLeft(hotkeyTotalPadding)}] {Text}";
            return string.IsNullOrWhiteSpace(HelpMessage)
                ? str : $"{str} - {HelpMessage}";
        }

        public static Choice[] CreateAll(Collection<ChoiceDescription> choices)
        {
            // If each choice description already specifies a hotkey, use those.
            if (choices.All(c => c.Label.Contains('&')))
            {
                return choices
                    .Select(c => new Choice(c.Label, c.HelpMessage))
                    .ToArray();
            }

            // Otherwise, create ordinal hotkeys.
            var choiceCount = choices.Count;
            var choiceObjects = new Choice[choiceCount];
            for (var i = 0; i < choiceCount; i++)
            {
                var choice = choices[i];
                choiceObjects[i] = new Choice(
                    choice.Label, (i + 1).ToString(), choice.HelpMessage);
            }
            return choiceObjects;
        }
    }
}
