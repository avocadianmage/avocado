using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.Engine.Modules;
using AvocadoShell.PowerShellService;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.Linq;
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
        bool exitRequested = false;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Scroll to the end each time a new line is added.
            LineAdded += (sender, e) => TextBase.ScrollToEnd();

            Task.Run(initPSEngine);
        }

        protected override void OnUnload(RoutedEventArgs e)
        {
            base.OnUnload(e);
            terminateExec();
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
                // Display error, if any.
                if (!string.IsNullOrWhiteSpace(e.Error))
                {
                    WriteLine(e.Error, Config.ErrorFontBrush);
                }

                // Exit if requested. Otherwise, display a new prompt.
                if (exitRequested) exit();
                else displayShellPrompt(e.Path);
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Always detect Ctrl+B break.
            if (IsControlKeyDown && e.Key == Key.B)
            {
                terminateExec();
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void HandleSpecialKeys(KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    if (TextBase.CaretPosition.Paragraph == null) break;
                    if (!isCaretDirectlyInFrontOfPrompt) break;
                    // The caret position does not change if text is selected
                    // (unless Shift+Left is pressed) so we should not 
                    // suppress that case.
                    e.Handled = TextBase.Selection.IsEmpty
                        || (e.Key == Key.Left && IsShiftKeyDown);
                    break;
                case Key.Home:
                    MoveCaret(-distanceToPromptEnd, IsShiftKeyDown);
                    e.Handled = true;
                    break;

                // Clear input.
                case Key.Escape:
                    clearInput();
                    break;

                // Input history.
                case Key.Up:
                case Key.Down:
                    inputHistoryLookup(e.Key == Key.Down);
                    e.Handled = true;
                    break;

                // Case autocompletion.
                case Key.Tab:
                    performTabCompletion();
                    e.Handled = true;
                    break;

                // Handle command execution.
                case Key.Enter:
                    execute();
                    e.Handled = true;
                    break;
            }

            // Ensure we are using the input color before processing regular
            // keys.
            base.HandleSpecialKeys(e);
        }

        void execute()
        {
            // Disable user input.
            InputEnabled = false;

            // Get user input.
            var input = getInput();

            // Position caret for writing command output.
            MoveCaretToDocumentEnd();
            WriteLine();

            // Signal to the powershell process that the we are done entering
            // input.
            resetEvent.Signal(input);

            // Quit if the input was entered due to a custom prompt in an 
            // executing process.
            if (!currentPrompt.FromShell) return;

            executeCommand(input);
        }

        void executeCommand(string input)
        {
            inputHistory.Add(input);
            psEngine.ExecuteCommand(input);
        }

        void performTabCompletion()
        {
            InputEnabled = false;
            
            var callback = new Action<string>((completion) =>
            {
                if (completion != null) replaceInput(completion);
                InputEnabled = true;
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

        public void RequestExit() => exitRequested = true;
        
        void exit()
        {
            Action action = () =>
            {
                InputEnabled = false;
                CloseWindow();
            };
            Dispatcher.BeginInvoke(action);
        }

        public string WritePrompt(string prompt)
        {
            var action = new Action<bool, string>(startPrompt);
            Dispatcher.BeginInvoke(action, false, prompt);
            return resetEvent.Block();
        }

        void displayShellPrompt(string path)
        {
            // Update text and window title displays.
            var shellPromptStr = Prompt.GetShellPromptStr(path);
            SetWindowTitle(shellPromptStr);
            startPrompt(true, shellPromptStr);
        }

        void startPrompt(bool fromShell, string text)
        {
            // Write prompt text.
            var brush = fromShell ? Config.PromptBrush : Config.SystemFontBrush;
            Write(text.TrimEnd(), brush);
            Write(" ", Foreground);

            // Update the current prompt object.
            currentPrompt = new Prompt(fromShell, CurrentLineString.Length);

            clearUndoBuffer();

            // Enable user input.
            InputEnabled = true;
        }

        void clearUndoBuffer()
        {
            TextBase.IsUndoEnabled = false;
            TextBase.IsUndoEnabled = true;
        }

        public void WriteCustom(string data, Brush foreground, bool newline)
        {
            safeWrite(data, foreground, newline);
        }

        public void WriteOutputLine(string data)
        {
            // Check if the line contains any ANSI codes that we should process.
            if (ANSICode.ContainsANSICodes(data))
            {
                var action = new Action<string>(writeOutputLineWithANSICodes);
                Dispatcher.BeginInvoke(action, data);
                return;
            }
            
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

        void writeOutputLineWithANSICodes(string data)
        {
            var segments = ANSICode.GetColorSegments(data);
            if (!segments.Any()) return;
            segments
                .Take(segments.Count - 1)
                .ForEach(seg => writeANSISegment(seg, false));
            writeANSISegment(segments.Last(), true);
        }

        void writeANSISegment(ANSISegment segment, bool newLine)
        {
            var text = segment.Text;
            var brush = segment.Brush ?? Config.SystemFontBrush;
            if (newLine) WriteLine(text, brush);
            else Write(text, brush);

        }
        
        string getInput() => CurrentLineString.Substring(currentPrompt.LinePos);

        void inputHistoryLookup(bool forward)
        {
            // Disallow lookup when not at the shell prompt.
            if (!currentPrompt.FromShell) return;

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