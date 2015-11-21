using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.MiscTools;

namespace AvocadoShell.Engine
{
    sealed class TerminalTextArea : InputTextArea, IShellUI
    {
        readonly InputHistory inputHistory = new InputHistory();
        readonly ResetEventWithData<string> resetEvent
            = new ResetEventWithData<string>();

        PSEngine psEngine;
        Prompt currentPrompt;
        
        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);
            Task.Run(initPSEngine);
        }

        async Task initPSEngine()
        {
            psEngine = new PSEngine(this);
            psEngine.ExecDone += onExecDone;
            await psEngine.InitEnvironment();
        }

        void onExecDone(object sender, ExecDoneEventArgs e)
        {
            Action action = () =>
            {
                if (!string.IsNullOrWhiteSpace(e.Error))
                {
                    WriteLine(e.Error, Config.ErrorFontBrush);
                }
                displayShellPrompt(e.Path);
            };
            Dispatcher.BeginInvoke(action);
        }

        void terminateExec()
        {
            // Terminate the powershell process.
            psEngine.Stop();

            // Ensure the powershell thread is unblocked.
            resetEvent.Signal(null);
        }

        protected override void OnUnload(RoutedEventArgs e)
        {
            base.OnUnload(e);
            terminateExec();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            
            // Always detect Ctrl+C break.
            if (IsControlKeyDown && e.Key == Key.C)
            {
                terminateExec();
                return;
            }

            // Ignore input if the InputEnabled flag is false.
            if (!InputEnabled)
            {
                e.Handled = true;
                return;
            }

            // Perform special handling for certain keys.
            switch (e.Key)
            {
                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    if (!isCaretDirectlyInFrontOfPrompt) break;
                    e.Handled = true;
                    SetDefaultForeground();
                    break;

                // Handle command execution.
                case Key.Enter:
                    e.Handled = true;
                    execute();
                    break;
            }
        }

        //protected override void OnHomeKeyDown(KeyEventArgs e)
        //{
        //    TextContent.Translate(1 - TextContent.CaretX);
        //}

        //protected override void OnUpKeyDown(KeyEventArgs e)
        //{
        //    // Look up and display the previous input in the command history.
        //    inputHistoryLookup(false);
        //}

        //protected override void OnDownKeyDown(KeyEventArgs e)
        //{
        //    // Look up and display the next input in the command history.
        //    inputHistoryLookup(true);
        //}

        void execute()
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

            executeCommand(input);
        }

        void prepareForOutput()
        {
            // Disable user input.
            InputEnabled = false;

            // Position caret for writing command output.
            MoveToDocumentEnd();
            WriteLine();
        }

        void executeCommand(string input)
        {
            inputHistory.Add(input);
            psEngine.ExecuteCommand(input);
        }

        //protected override void OnEscapeKeyDown(KeyEventArgs e)
        //{
        //    clearInput();
        //}

        //protected override void OnTabKeyDown(KeyEventArgs e)
        //{
        //    performTabCompletion();
        //}

        //void performTabCompletion()
        //{
        //    InputEnabled = false;

        //    var input = getInput();
        //    var index = TextContent.CaretX - 1;
        //    var forward = !IsShiftKeyDown;

        //    var callback = new Action<string>((completion) =>
        //    {
        //        if (completion != null) replaceInput(completion);
        //        InputEnabled = true;
        //    });

        //    getCompletion(input, index, forward, callback);
        //}
    
        void getCompletion(
            string input,
            int index,
            bool forward,
            Action<string> callback)
        {
            Task.Run(() => psEngine.GetCompletion(input, index, forward))
                .ContinueWith(
                    task => callback(task.Result), 
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        bool isCaretDirectlyInFrontOfPrompt 
            => (CaretX <= currentPrompt.LinePos);

        public void Exit()
        {
            Action action = () =>
            {
                InputEnabled = false;
                CloseWindow();
            };
            Dispatcher.BeginInvoke(action);
        }

        public string ReadLine()
        {
            var action = new Action<bool>(displayPrompt);
            Dispatcher.BeginInvoke(action, false);
            return resetEvent.Block();
        }

        void displayShellPrompt(string path)
        {
            // Do not display a new prompt if the window is closing.
            if (IsWindowClosing) return;

            // Update text and window title displays.
            var shellPromptStr = Prompt.GetShellPromptStr(path);
            if (!string.IsNullOrEmpty(CurrentLineString))
            {
                // If there is text on this line, go to a new line.
                WriteLine();
            }
            Write(shellPromptStr, Config.PromptBrush);
            SetWindowTitle(shellPromptStr);

            displayPrompt(true);
        }

        void displayPrompt(bool fromShell)
        {
            // Update the current prompt object.
            currentPrompt = new Prompt(fromShell, CurrentLineString.Length);

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
                : new Action<string, Brush>(Write);
            Dispatcher.BeginInvoke(action, data, foreground);
        }

        string getInput() => CurrentLineString.Substring(currentPrompt.LinePos);

        //void inputHistoryLookup(bool forward)
        //{
        //    // Save the current user input to the buffer.
        //    inputHistory.SaveInput(getInput());

        //    // Look up the stored input to display from the buffer.
        //    var storedInput = inputHistory.Cycle(forward);

        //    // Return if no command was found.
        //    if (storedInput == null) return;

        //    // Update the display to show the new input.
        //    replaceInput(storedInput);
        //}

        //void replaceInput(string replacement)
        //{
        //    var commonSubstring = getInput().CommonStart(replacement);
        //    var commonLength = commonSubstring.Length;

        //    // Remove from the display the input that is changing.
        //    TextContent.Translate(-TextContent.CaretX + commonLength + 1);
        //    TextContent.DeleteToEnd();

        //    // Write the new part of the replacement input.
        //    var changingSubstring = replacement.Substring(commonLength);
        //    WriteInput(changingSubstring);
        //}

        //void clearInput()
        //{
        //    replaceInput(string.Empty);
        //}
    }
}