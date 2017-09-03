using AvocadoShell.PowerShellService.Host.ChoicePrompt;
using AvocadoShell.UI;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Windows.Media;

namespace AvocadoShell.PowerShellService.Host
{
    sealed class CustomHostUI 
        : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
    {
        readonly IShellUI shellUI;
        readonly List<ProgressRecord> actionsInProgress
            = new List<ProgressRecord>();

        public CustomHostUI(IShellUI shellUI) => this.shellUI = shellUI;

        void writeLineUnlessWhitespace(string line, Brush foreground)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            shellUI.WriteCustom(line, foreground, true);
        }

        void writePromptPreamble(string caption, string message)
        {
            writeLineUnlessWhitespace(caption, Config.OutputBrush);
            writeLineUnlessWhitespace(message, Config.VerboseBrush);
        }

        void writeChoicePromptPreamble(
            string caption,
            string message,
            Choice[] choiceList,
            IEnumerable<int> defaultChoices)
        {
            writePromptPreamble(caption, message);

            // Output list of choices.
            var choiceCount = choiceList.Length;
            var hotkeyTotalPadding = choiceList.Max(c => c.Hotkey.Length);
            for (var i = 0; i < choiceCount; i++)
            {
                var brush = defaultChoices.Contains(i)
                    ? Config.SelectedBrush : Config.OutputBrush;
                var text = choiceList[i].ToDisplayLine(hotkeyTotalPadding);
                shellUI.WriteCustom(text, brush, true);
            }
        }

        string writeChoicePrompt(bool isMultiOption, bool hasDefault)
        {
            var basePromptStr = isMultiOption ? "Multiselect" : "Select";
            shellUI.WriteCustom(
                basePromptStr, Config.PromptBrush, false);
            if (hasDefault)
            {
                shellUI.WriteCustom(
                    " (leave blank for ", Config.PromptBrush, false);
                shellUI.WriteCustom("default", Config.SelectedBrush, false);
                shellUI.WriteCustom(")", Config.PromptBrush, false);
            }
            return shellUI.WritePrompt(": ");
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
        /// <param name="descriptions">A collection of FieldDescription objects 
        /// that describe each field of the prompt.</param>
        /// <returns>A dictionary object that contains the results of the user 
        /// prompts.</returns>
        public override Dictionary<string, PSObject> Prompt(
            string caption,
            string message,
            Collection<FieldDescription> descriptions)
        {
            writePromptPreamble(caption, message);

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

                // The only way input can be null is if execution was stppped.
                results[desc.Name] = input == null 
                    ? null : PSObject.AsPSObject(input);
            }
            return results;
        }

        /// <summary>
        /// Provides a set of choices that enables the user to choose a single
        /// option.
        /// </summary>
        /// <param name="caption">Text that precedes the choices.</param>
        /// <param name="message">A message that describes the choices.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that 
        /// describe each choice.</param>
        /// <param name="defaultChoice">The index of the label in the choices 
        /// parameter collection. To indicate no default choice, set to -1.
        /// </param>
        /// <returns>The index of the choices parameter collection element that 
        /// corresponds to the option that is selected by the user.</returns>
        public override int PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            var choiceObjects = Choice.CreateAll(choices);
            writeChoicePromptPreamble(
                caption, message, choiceObjects, defaultChoice.Yield());
            return InputParser.SingleChoicePrompt(
                () => writeChoicePrompt(false, defaultChoice != -1),
                choiceObjects, 
                defaultChoice);
        }

        /// <summary>
        /// Provides a set of choices that enables the user to choose a multiple
        /// options.
        /// </summary>
        /// <param name="caption">Text that precedes the choices.</param>
        /// <param name="message">A message that describes the choices.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that 
        /// describe each choice.</param>
        /// <param name="defaultChoice">A list of indexes of the labels in the 
        /// choices parameter collection. To indicate no default choices, set to 
        /// null.
        /// </param>
        /// <returns>The indexes of the choices parameter collection element 
        /// that corresponds to the options that are selected by the user.
        /// </returns>
        public Collection<int> PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            IEnumerable<int> defaultChoices)
        {
            var choiceObjects = Choice.CreateAll(choices);
            writeChoicePromptPreamble(
                caption, message, choiceObjects, defaultChoices);
            return new Collection<int>(
                InputParser.MultiChoiceNumericPrompt(
                    () => writeChoicePrompt(true, defaultChoices.Any()), 
                    choiceObjects.Length, 
                    defaultChoices)
                .ToList());
        }

        /// <summary>
        /// Prompts the user for credentials with a specified prompt caption, 
        /// prompt message, user name, and target name.
        /// </summary>
        /// <param name="caption">The caption for the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be 
        /// prompted for.</param>
        /// <param name="targetName">The name of the target for which the 
        /// credential is collected.</param>
        /// <returns>PSCredential object storing the credentials entered by the
        /// user.</returns>
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
        /// Prompts the user for credentials with a specified prompt caption, 
        /// prompt message, user name, target name, credential types allowed to 
        /// be returned, and UI behavior options.
        /// </summary>
        /// <param name="caption">The caption for the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be 
        /// prompted for.</param>
        /// <param name="targetName">The name of the target for which the 
        /// credential is collected.</param>
        /// <param name="allowedCredentialTypes">A PSCredentialTypes constant 
        /// that identifies the type of credentials that can be returned.
        /// </param>
        /// <param name="options">A PSCredentialUIOptions constant that 
        /// identifies the UI behavior when it gathers the credentials.</param>
        /// <returns>PSCredential object storing the credentials entered by the
        /// user.</returns>
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
        public override string ReadLine() => shellUI.WritePrompt(string.Empty);

        /// <summary>
        /// Reads characters entered by the user until a newline (carriage 
        /// return) is encountered and returns the characters as a secure 
        /// string.
        /// </summary>
        /// <returns>A secure string containing the input entered by the user.
        /// </returns>
        public override SecureString ReadLineAsSecureString() 
            => shellUI.WriteSecurePrompt(string.Empty);

        /// <summary>
        /// Writes characters to the output display of the host.
        /// </summary>
        /// <param name="value">The characters to be written.</param>
        public override void Write(string value) => baseWrite(value, false);

        /// <summary>
        /// Writes a line of characters to the output display of the host and
        /// appends a newline character(carriage return). 
        /// </summary>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(string value) => baseWrite(value, true);
        public override void WriteLine() => WriteLine(string.Empty);

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
            baseWriteColor(foregroundColor, backgroundColor, value, false);
        }

        public override void WriteLine(
            ConsoleColor foregroundColor, 
            ConsoleColor backgroundColor, 
            string value)
        {
            baseWriteColor(foregroundColor, backgroundColor, value, true);
        }

        void baseWrite(string value, bool newline)
        {
            baseWriteColor(
                Config.SystemConsoleForeground, 
                default(ConsoleColor), 
                value,
                newline);
        }

        void baseWriteColor(
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor,
            string value,
            bool newline)
        {
            var brush = consoleColorToBrush(foregroundColor);
            shellUI.WriteCustom(value, brush, newline);
        }

        static Brush consoleColorToBrush(ConsoleColor consoleColor)
        {
            // Recognize default system color.
            if (consoleColor == Config.SystemConsoleForeground)
                return Config.OutputBrush;

            // Handle 'DarkYellow' which does not have a brush with a matching
            // name.
            if (consoleColor == ConsoleColor.DarkYellow) return Brushes.Orange;
            
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
        /// Writes a progress report to the output display of the host.
        /// </summary>
        /// <param name="sourceId">Unique identifier of the source of the 
        /// record.</param>
        /// <param name="record">A ProgressReport object.</param>
        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (record.RecordType == ProgressRecordType.Completed) return;

            var line = $"{record.Activity}: {record.StatusDescription}";
            var percent = record.PercentComplete;
            if (percent >= 0) line += $" ({percent}%)";
            shellUI.WriteCustom(line, Config.ProgressBrush, true);
        }

        /// <summary>
        /// Writes a debug message to the output display of the host.
        /// </summary>
        /// <param name="message">The debug message that is displayed.</param>
        public override void WriteDebugLine(string message)
            => shellUI.WriteCustom(
                $"[Debug] {message}", Config.DebugBrush, true);

        /// <summary>
        /// Writes a verbose message to the output display of the host.
        /// </summary>
        /// <param name="message">The verbose message that is displayed.</param>
        public override void WriteVerboseLine(string message)
            => shellUI.WriteCustom(
                $"[Verbose] {message}", Config.VerboseBrush, true);

        /// <summary>
        /// Writes a warning message to the output display of the host.
        /// </summary>
        /// <param name="message">The warning message that is displayed.</param>
        public override void WriteWarningLine(string message)
            => shellUI.WriteCustom(
                $"[Warning] {message}", Config.WarningBrush, true);
    }
}
