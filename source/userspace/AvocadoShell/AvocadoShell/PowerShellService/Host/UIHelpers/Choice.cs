using StandardLibrary.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;

namespace AvocadoShell.PowerShellService.Host.UIHelpers
{
    sealed class Choice
    {
        public static string GetPromptText(
            IEnumerable<Choice> choiceList, string defaultHotkey)
        {
            var builder = new StringBuilder();
            choiceList.ForEach(c => builder.Append(c));
            builder.Append($"[?] Help  (default is \"{defaultHotkey}\"): ");
            return builder.ToString();
        }

        public static string GetHelpText(IEnumerable<Choice> choiceList)
        {
            var builder = new StringBuilder();
            choiceList.ForEach(c => builder.AppendLine(c.HelpLine));
            return builder.ToString();
        }

        public string HelpMessage { get; }
        public string Text { get; }
        public string Hotkey { get; }

        public Choice(ChoiceDescription desc)
        {
            Text = desc.Label.Replace("&", string.Empty);
            Hotkey = desc.Label.Split('&')[1].First().ToString().ToUpper();
            HelpMessage = desc.HelpMessage;
        }

        public string HelpLine => $"{Hotkey} - {HelpMessage}";

        public override string ToString() => $"[{Hotkey}] {Text}  ";
    }
}
