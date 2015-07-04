using AvocadoFramework.Controls.Text.Input;
using AvocadoShell.PowerShellService;
using AvocadoUtilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.MiscTools;

namespace AvocadoShell.Engine
{
    sealed class TerminalControl : InputControl, IShellUI
    {
        readonly ShellEnvironment shellEnv;
        readonly InputHistory inputHistory = new InputHistory();
        readonly ResetEventWithData<string> resetEvent
            = new ResetEventWithData<string>();

        Prompt currentPrompt;

        public TerminalControl()
        {
            shellEnv = new ShellEnvironment(this);
        }

        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);
            shellEnv.Init();
        }

        void terminateExec()
        {
            // Ensure the powershell thread is unblocked.
            resetEvent.Signal(null);

            // Terminate the powershell process.
            shellEnv.StopExecution();
        }

        protected override void OnUnload(RoutedEventArgs e)
        {
            base.OnUnload(e);
            terminateExec();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Detect Ctrl+C break.
            if (IsControlKeyDown && e.Key == Key.C)
            {
                terminateExec();
                return;
            }
            
            base.OnKeyDown(e);
        }

        protected override void OnBackKeyDown(KeyEventArgs e)
        {
            // Do not allow the prompt to be erased.
            if (isCaretDirectlyInFrontOfPrompt) return;

            // Otherwise, process the key press as usual.
            base.OnBackKeyDown(e);
        }

        protected override void OnLeftKeyDown(KeyEventArgs e)
        {
            // Do not allow the caret to reach the prompt text.
            if (isCaretDirectlyInFrontOfPrompt) return;

            // Otherwise, process the key press as usual.
            base.OnLeftKeyDown(e);
        }

        protected override void OnHomeKeyDown(KeyEventArgs e)
        {
            TextContent.Translate(1 - TextContent.CaretX);
        }

        protected override void OnUpKeyDown(KeyEventArgs e)
        {
            // Look up and display the previous input in the command history.
            inputHistoryLookup(false);
        }

        protected override void OnDownKeyDown(KeyEventArgs e)
        {
            // Look up and display the next input in the command history.
            inputHistoryLookup(true);
        }

        protected override void OnEnterKeyDown(KeyEventArgs e)
        {
            // Get user input.
            var input = getInput();

            prepareForOutput();

            // Signal to the powershell process that the we are done entering
            // input.
            resetEvent.Signal(input);

            // Quit if the input was entered due to a custom prompt in an 
            // executing process.
            if (!currentPrompt.FromShell) return;

            execute(input);
        }

        void execute(string input)
        {
            inputHistory.Add(input);
            shellEnv.Execute(input);
        }

        void prepareForOutput()
        {
            // Disable user input.
            InputEnabled = false;

            // Position caret for writing command output.
            TextContent.TranslateToDocumentEnd();
            TextContent.InsertLineBreak();
        }

        protected override void OnEscapeKeyDown(KeyEventArgs e)
        {
            clearInput();
        }

        protected override void OnTabKeyDown(KeyEventArgs e)
        {
            performTabCompletion();
        }

        void performTabCompletion()
        {
            InputEnabled = false;

            var input = getInput();
            var index = TextContent.CaretX - 1;
            var forward = !IsShiftKeyDown;

            var callback = new Action<string>((completion) =>
            {
                if (completion != null) replaceInput(completion);
                InputEnabled = true;
            });

            getCompletion(input, index, forward, callback);
        }
    
        void getCompletion(
            string input,
            int index,
            bool forward,
            Action<string> callback)
        {
            Task.Run<string>(
                () => shellEnv.GetCompletion(input, index, forward))
            .ContinueWith(
                (task) => callback(task.Result), 
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        bool isCaretDirectlyInFrontOfPrompt
        {
            get 
            {
                var promptLen = currentPrompt.Text.Length;
                return TextContent.CaretX <= Math.Sign(promptLen); 
            }
        }

        public void Exit()
        {
            Dispatcher.BeginInvoke(new Action(exit));
        }

        void exit()
        {
            InputEnabled = false;
            CloseWindow();
        }

        public void DisplayShellPrompt(string path)
        {
            safeDisplayPrompt(Prompt.CreateShellPrompt(path));
        }

        public string DisplayPrompt(string str)
        {
            safeDisplayPrompt(Prompt.CreateOtherPrompt(str));
            return resetEvent.Block();
        }

        void safeDisplayPrompt(Prompt prompt)
        {
            var action = new Action<Prompt>(displayPrompt);
            Dispatcher.BeginInvoke(action, prompt);
        }

        void displayPrompt(Prompt prompt)
        {
            // Do not allow any user input if the window is closing.
            if (IsWindowClosing) return;

            // Set the current prompt object to the new prompt.
            currentPrompt = prompt;

            // If there is text on this line, go to a new line.
            if (!string.IsNullOrEmpty(TextContent.GetCurrentLineText()))
            {
                TextContent.InsertLineBreak();
            }

            // Write prompt text.
            var foreground = prompt.FromShell 
                ? Config.PromptBrush : Config.SystemFontBrush;
            TextContent.InsertText(prompt.Text, foreground);

            // If we are displaying the shell prompt, also update the window
            // with the working directory.
            if (prompt.FromShell) SetWindowTitle(prompt.Text);

            // Enable user input.
            InputEnabled = true;
        }

        public void WriteCustom(string data, Brush foreground, bool newline)
        {
            safeWrite(data, foreground, newline);
        }

        public void WriteSystemLine(string data)
        {
            safeWrite(data, Config.SystemFontBrush, true);
        }

        public void WriteErrorLine(string data)
        {
            safeWrite(data, Config.ErrorFontBrush, true);
        }

        void safeWrite(string data, Brush foreground, bool newline)
        {
            var action = newline
                ? new Action<string, Brush>(WriteLine)
                : new Action<string, Brush>(TextContent.InsertText);
            Dispatcher.BeginInvoke(action, data, foreground);
        }

        string getInput()
        {
            var lineText = TextContent.GetCurrentLineText();
            return lineText.Substring(currentPrompt.Text.Length);
        }

        void inputHistoryLookup(bool forward)
        {
            // Save the current user input to the buffer.
            inputHistory.SaveInput(getInput());

            // Look up the stored input to display from the buffer.
            var storedInput = inputHistory.Cycle(forward);

            // Return if no command was found.
            if (storedInput == null) return;

            // Update the display to show the new input.
            replaceInput(storedInput);
        }

        void replaceInput(string replacement)
        {
            var commonSubstring = getInput().CommonStart(replacement);
            var commonLength = commonSubstring.Length;

            // Remove from the display the input that is changing.
            TextContent.Translate(-TextContent.CaretX + commonLength + 1);
            TextContent.DeleteToEnd();

            // Write the new part of the replacement input.
            var changingSubstring = replacement.Substring(commonLength);
            WriteInput(changingSubstring);
        }

        void clearInput()
        {
            replaceInput(string.Empty);
        }
    }
}