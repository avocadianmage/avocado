using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService;
using System;
using System.Threading.Tasks;
using System.Windows;
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
        bool inputEnabled;
        
        protected override void OnLoad(RoutedEventArgs e)
        {
            base.OnLoad(e);

            // Diable undo feature for the terminal.
            TextBase.IsUndoEnabled = false;

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
            // Always detect Ctrl+Z break.
            if (IsControlKeyDown && e.Key == Key.Z)
            {
                terminateExec();
                return;
            }
            
            // Ignore input if the InputEnabled flag is false.
            if (!inputEnabled)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);

            // Perform special handling for certain keys.
            switch (e.Key)
            {
                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    if (!isCaretDirectlyInFrontOfPrompt) break;
                    // The caret position does not change if text is selected
                    // (unless Shift+Left is pressed) so we should not 
                    // suppress that case.
                    e.Handled = TextBase.Selection.IsEmpty
                        || (e.Key == Key.Left && IsShiftKeyDown);
                    break;
                case Key.Home:
                    e.Handled = true;
                    MoveCaret(-distanceToPromptEnd, IsShiftKeyDown);
                    break;

                // Clear input.
                case Key.Escape:
                    clearInput();
                    break;

                // Input history.
                case Key.Up:
                case Key.Down:
                    e.Handled = true;
                    inputHistoryLookup(e.Key == Key.Down);
                    break;

                // Case autocompletion.
                case Key.Tab:
                    e.Handled = true;
                    performTabCompletion();
                    break;

                // Handle command execution.
                case Key.Enter:
                    e.Handled = true;
                    execute();
                    break;
            }

            // Ensure we are using the input color before actually processing 
            // the keypress.
            SetDefaultForeground();
        }

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
            inputEnabled = false;

            // Position caret for writing command output.
            MoveCaretToDocumentEnd();
            WriteLine();
        }

        void executeCommand(string input)
        {
            inputHistory.Add(input);
            psEngine.ExecuteCommand(input);
        }

        void performTabCompletion()
        {
            inputEnabled = false;
            
            var callback = new Action<string>((completion) =>
            {
                if (completion != null) replaceInput(completion);
                inputEnabled = true;
            });

            getCompletion(
                getInput(), 
                distanceToPromptEnd, 
                !IsShiftKeyDown, 
                callback);
        }

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
                inputEnabled = false;
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
            inputEnabled = true;
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
            clearInput();
            Write(replacement, Foreground);
        }

        void clearInput()
        {
            MoveCaretToDocumentEnd();
            TextBase.CaretPosition.DeleteTextInRun(-distanceToPromptEnd);
        }

        int distanceToPromptEnd => CaretX - currentPrompt.LinePos;
    }
}