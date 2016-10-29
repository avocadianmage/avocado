using AvocadoShell.Terminal;
using StandardLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Windows.Media;

namespace AvocadoShell.PowerShellService.Host
{
    /// <summary>
    /// A sample implementation of the PSHostUserInterface abstract class for 
    /// console applications. Not all members are implemented. Those that are 
    /// not implemented throw a NotImplementedException exception or return 
    /// nothing. Members that are implemented include those that map easily to 
    /// Console APIs and a basic implementation of the prompt API provided. 
    /// </summary>
    class CustomHostUI : PSHostUserInterface
    {
        readonly IShellUI shellUI;
        readonly List<ProgressRecord> actionsInProgress
            = new List<ProgressRecord>();

        public CustomHostUI(IShellUI shellUI)
        {
            this.shellUI = shellUI;
        }

        void writeLineUnlessWhitespace(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;
            shellUI.WriteOutputLine(data);
        }

        /// <summary>
        /// Gets an instance of the PSRawUserInterface object for this host
        /// application.
        /// </summary>
        public override PSHostRawUserInterface RawUI { get; } 
            = new CustomRawHostUI();

        /// <summary>
        /// Prompts the user for input. 
        /// <param name="caption">The caption or title of the prompt.</param>
        /// <param name="message">The text of the prompt.</param>
        /// <param name="descriptions">A collection of FieldDescription objects that 
        /// describe each field of the prompt.</param>
        /// <returns>A dictionary object that contains the results of the user 
        /// prompts.</returns>
        public override Dictionary<string, PSObject> Prompt(
            string caption,
            string message,
            Collection<FieldDescription> descriptions)
        {
            writeLineUnlessWhitespace(caption);
            writeLineUnlessWhitespace(message);

            var results = new Dictionary<string, PSObject>();
            foreach (var desc in descriptions)
            {
                // Get the correct prompting function depending on the 
                // FieldDescription (supports -AsSecureString).
                var expectedType = Type.GetType(desc.ParameterTypeFullName);
                var promptFunc = expectedType == typeof(SecureString)
                    ? (Func<string, object>)shellUI.WriteSecurePrompt
                    : shellUI.WritePrompt;

                // Prompt the user for input and store it in the return
                // dictionary.
                var input = promptFunc($"{desc.Name}: ");

                // (The only way input can be null is if execution was stppped.)
                results[desc.Name] = input == null 
                    ? null : PSObject.AsPSObject(input);
            }
            return results;
        }

        /// <summary>
        /// Provides a set of choices that enable the user to choose a 
        /// single option from a set of options. 
        /// </summary>
        /// <param name="caption">Text that proceeds (a title) the choices.</param>
        /// <param name="message">A message that describes the choice.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that describe 
        /// each choice.</param>
        /// <param name="defaultChoice">The index of the label in the Choices parameter 
        /// collection. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that corresponds 
        /// to the option that is selected by the user.</returns>
        public override int PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            writeLineUnlessWhitespace(caption);
            writeLineUnlessWhitespace(message);

            // Format the overall choice prompt string to display.
            var promptData = choices.Select(c => new Choice(c)).ToArray();
            var sb = new StringBuilder();
            promptData.ForEach(l => sb.Append($"[{l.Hotkey}] {l.Text}  "));
            sb.Append($"[?] Help (default is \"{promptData[defaultChoice].Hotkey}\"): ");

            // Read prompts until a match is made or the default is chosen.
            while (true)
            {
                var input = shellUI.WritePrompt(sb.ToString());

                // If the choice string was empty, use the default selection.
                if (string.IsNullOrWhiteSpace(input)) return defaultChoice;

                // See if the selection matched and return the corresponding
                // index if it did.
                input = input.Trim().ToUpper();
                for (var i = 0; i < choices.Count; i++)
                {
                    if (promptData[i].Hotkey == input) return i;
                }
            }
        }

        sealed class Choice
        {
            public string HelpMessage { get; }
            public string Text { get; }
            public string Hotkey { get; }

            public Choice(ChoiceDescription desc)
            {
                HelpMessage = desc.HelpMessage;
                Text = desc.Label;
                var pieces = Text.Split('&');

                // If hotkey is not available or cannot be determined, just return
                // the input as the label.
                if (pieces.Length != 2) return;

                // Return the input string without the ampersand.
                Text = string.Join(string.Empty, pieces);

                // Set the hotkey as the first character after the ampersand.
                var hotkeyPiece = pieces[1];
                if (!string.IsNullOrWhiteSpace(hotkeyPiece))
                    Hotkey = hotkeyPiece.First().ToString().ToUpper();
            }
        }

        /// <summary>
        /// Prompts the user for credentials with a specified prompt window caption, 
        /// prompt message, user name, and target name. In this example this 
        /// functionality is not needed so the method throws a 
        /// NotImplementException exception.
        /// </summary>
        /// <param name="caption">The caption for the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override PSCredential PromptForCredential(
            string caption, string message, string userName, string targetName)
        {
            // Quit if no username was specified.
            if (string.IsNullOrWhiteSpace(userName)) return null;

            var prompt = $"Password for {userName}: ";
            var password = shellUI.WriteSecurePrompt(prompt);
            return new PSCredential(userName, password);
        }

        /// <summary>
        /// Prompts the user for credentials by using a specified prompt window caption, 
        /// prompt message, user name and target name, credential types allowed to be 
        /// returned, and UI behavior options. In this example this functionality 
        /// is not needed so the method throws a NotImplementException exception.
        /// </summary>
        /// <param name="caption">The caption for the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <param name="allowedCredentialTypes">A PSCredentialTypes constant that 
        /// identifies the type of credentials that can be returned.</param>
        /// <param name="options">A PSCredentialUIOptions constant that identifies the UI 
        /// behavior when it gathers the credentials.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options)
        {
            return PromptForCredential(caption, message, userName, targetName);
        }


        /// <summary>
        /// Reads characters that are entered by the user until a newline 
        /// (carriage return) is encountered.
        /// </summary>
        /// <returns>The characters that are entered by the user.</returns>
        public override string ReadLine() => shellUI.WritePrompt("Prompt: ");

        /// <summary>
        /// Reads characters entered by the user until a newline (carriage return) 
        /// is encountered and returns the characters as a secure string. In this 
        /// example this functionality is not needed so the method throws a 
        /// NotImplementException exception.
        /// </summary>
        /// <returns>Throws a NotImplemented exception.</returns>
        public override SecureString ReadLineAsSecureString()
            => shellUI.WriteSecurePrompt("Secure prompt: ");

        /// <summary>
        /// Writes characters to the output display of the host.
        /// </summary>
        /// <param name="value">The characters to be written.</param>
        public override void Write(string value)
            => shellUI.WriteCustom(value, Config.SystemFontBrush, false);

        /// <summary>
        /// Writes characters to the output display of the host with possible 
        /// foreground and background colors. 
        /// </summary>
        /// <param name="foregroundColor">The color of the characters.</param>
        /// <param name="backgroundColor">The backgound color to use.</param>
        /// <param name="value">The characters to be written.</param>
        public override void Write(
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor,
            string value)
        {
            var brush = consoleColorToBrush(foregroundColor);
            shellUI.WriteCustom(value, brush, false);
        }

        static Brush consoleColorToBrush(ConsoleColor consoleColor)
        {
            // Recognize default system color.
            if (consoleColor == Config.DefaultConsoleColor)
            {
                return Config.SystemFontBrush;
            }

            // Handle 'DarkYellow' which does not have a brush with a matching
            // name.
            if (consoleColor == ConsoleColor.DarkYellow)
            {
                return Brushes.DarkGoldenrod;
            }
            
            var colorStr = consoleColor.ToString();
            return new BrushConverter().ConvertFromString(colorStr) as Brush;
        }

        /// <summary>
        /// Writes an error message to the output display of the host.
        /// </summary>
        /// <param name="value">The error message that is displayed.</param>
        public override void WriteErrorLine(string value)
            => shellUI.WriteErrorLine(value);

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// and appends a newline character(carriage return). 
        /// </summary>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(string value)
            => shellUI.WriteOutputLine(value);

        /// <summary>
        /// Writes a progress report to the output display of the host.
        /// </summary>
        /// <param name="sourceId">Unique identifier of the source of the record.</param>
        /// <param name="record">A ProgressReport object.</param>
        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (record.RecordType == ProgressRecordType.Completed)
            {
                actionsInProgress.Remove(record);
                return;
            }
            
            if (actionsInProgress.Contains(record)) return;

            actionsInProgress.Add(record);
            shellUI.WriteOutputLine(
                $"{record.Activity} - {record.StatusDescription}");
        }

        /// <summary>
        /// Writes a debug message to the output display of the host.
        /// </summary>
        /// <param name="message">The debug message that is displayed.</param>
        public override void WriteDebugLine(string message)
        {
            shellUI.WriteCustom($"[Debug] {message}", Brushes.Cyan, true);
        }

        /// <summary>
        /// Writes a verbose message to the output display of the host.
        /// </summary>
        /// <param name="message">The verbose message that is displayed.</param>
        public override void WriteVerboseLine(string message)
        {
            shellUI.WriteCustom(
                $"[Verbose] {message}", Brushes.DimGray, true);
        }

        /// <summary>
        /// Writes a warning message to the output display of the host.
        /// </summary>
        /// <param name="message">The warning message that is displayed.</param>
        public override void WriteWarningLine(string message)
        {
            shellUI.WriteCustom($"[Warning] {message}", Brushes.Orange, true);
        }
    }
}
