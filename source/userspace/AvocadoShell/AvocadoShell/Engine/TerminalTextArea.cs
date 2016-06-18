using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.Engine.Modules;
using AvocadoShell.PowerShellService;
using AvocadoShell.PowerShellService.Runspaces;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.MiscTools;
using UtilityLib.Processes;

namespace AvocadoShell.Engine
{
    sealed class TerminalTextArea : InputTextArea, IShellUI
    {
        readonly PowerShellEngine psEngine;
        readonly InputHistory inputHistory = new InputHistory();
        readonly ResetEventWithData<string> resetEvent
            = new ResetEventWithData<string>();
        readonly Prompt currentPrompt = new Prompt();

        public TerminalTextArea()
        {
            psEngine = createPowerShellEngine();

            Unloaded += (s, e) => terminateExec();
        }

        PowerShellEngine createPowerShellEngine()
        {
            var psEngine = new PowerShellEngine(this);
            psEngine.ExecDone += onExecDone;
            psEngine.ExitRequested += (s, e)
                => Dispatcher.BeginInvoke(new Action(exit));
            return psEngine;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Scroll to the end each time a new line is added.
            LineAdded += (sender, e) => TextBase.ScrollToEnd();
            
            psEngine.InitEnvironment();
        }
        
        void onExecDone(object sender, ExecDoneEventArgs e)
        {
            // Display error, if any.
            if (!string.IsNullOrWhiteSpace(e.Error)) WriteErrorLine(e.Error);

            // Show the next shell prompt.
            var shellPromptStr = getShellPromptString().Result;
            Dispatcher.BeginInvoke(
                new Action(() => displayShellPrompt(shellPromptStr)));
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
            if (e.Handled) return;

            switch (e.Key)
            {
                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    if (TextBase.CaretPosition.Paragraph == null) break;
                    if (!isAtOrBeforePrompt(TextBase.CaretPosition)) break;
                    // The caret position does not change if text is selected
                    // (unless Shift+Left is pressed) so we should not 
                    // suppress that case.
                    e.Handled = TextBase.Selection.IsEmpty
                        || (e.Key == Key.Left && IsShiftKeyDown);
                    break;
                case Key.Home:
                    if (!IsControlKeyDown)
                    {
                        var lineStart
                            = TextBase.CaretPosition.GetLineStartPosition(0);
                        if (!isAtOrBeforePrompt(lineStart)) break;
                    }
                    // If we are on the same line as the prompt, move the cursor
                    // to the end of the prompt instead of the beginning of the
                    // line.
                    MoveCaret(-distanceToPromptEnd, IsShiftKeyDown);
                    e.Handled = true;
                    break;
                case Key.A:
                    if (!IsControlKeyDown) break;
                    // Ctrl+A will select all text after the prompt.
                    MoveCaretToDocumentEnd();
                    TextBase.Selection.Select(
                        TextBase.CaretPosition.GetPositionAtOffset(
                            -distanceToPromptEnd),
                        TextBase.CaretPosition);
                    e.Handled = true;
                    break;

                // Currently no support for PageUp/PageDown.
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = true;
                    break;

                // Clear input or selection.
                case Key.Escape:
                    if (TextBase.Selection.IsEmpty) clearInput();
                    else ClearSelection();
                    e.Handled = true;
                    break;

                // Input history.
                case Key.Up:
                case Key.Down:
                    inputHistoryLookup(e.Key == Key.Down);
                    e.Handled = true;
                    break;

                // Autocompletion.
                case Key.Tab:
                    performAutocomplete();
                    e.Handled = true;
                    break;

                // Handle command execution.
                case Key.Enter:
                    // Go to new line if shift is pressed at shell prompt.
                    if (IsShiftKeyDown) break;
                    // Otherwise, execute the input.
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
            EnableInput(false);

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
            // Add command to history.
            inputHistory.Add(input);

            // Otherwise, use PowerShell to interpret the command.
            psEngine.ExecuteCommand(input);
        }
        
        public async Task<bool> RunNativeCommand(string message)
            => await psEngine.RunNativeCommand(message);
        
        void performAutocomplete()
        {
            EnableInput(false);

            var input = getInput();
            var index = distanceToPromptEnd;
            var forward = !IsShiftKeyDown;

            Task.Run(() => psEngine.GetCompletion(input, index, forward))
                .ContinueWith(task =>
                {
                    var completion = task.Result;
                    if (completion != null) replaceInput(completion);
                    EnableInput(true);
                }, 
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        void exit()
        {
            EnableInput(false);
            CloseWindow();
        }

        public string WritePrompt(string prompt) => writePrompt(prompt, false);

        public SecureString WriteSecurePrompt(string prompt)
        {
            var input = writePrompt(prompt, true);
            var secureStr = new SecureString();
            input.ForEach(secureStr.AppendChar);
            secureStr.MakeReadOnly();
            return secureStr;
        }

        string writePrompt(string prompt, bool secure)
        {
            var action = new Action<string, bool, bool>(startPrompt);
            Dispatcher.BeginInvoke(action, prompt, false, secure);
            return resetEvent.Block();
        }

        void displayShellPrompt(string text)
        {
            SetWindowTitle(text);
            startPrompt(text, true, false);
        }

        async Task<string> getShellPromptString()
        {
            var prompt = string.Empty;

            // Add root indication.
            if (EnvUtils.IsAdmin) prompt += "(root) ";

            // Add remote computer name.
            if (psEngine.RemoteComputerName != null)
            {
                prompt += $"[{psEngine.RemoteComputerName}] ";
            }

            // Add working directory.
            prompt += $"{await psEngine.GetWorkingDirectory()} ";

            return prompt;
        }

        void startPrompt(string text, bool fromShell, bool secure)
        {
            // Write prompt text.
            var brush = fromShell ? Config.PromptBrush : Config.SystemFontBrush;
            Write(text.TrimEnd(), brush);
            Write(" ", secure ? Brushes.Transparent : Foreground);

            // Update the current prompt object.
            currentPrompt.Update(fromShell, CurrentLineString);

            clearUndoBuffer();

            // Enable user input.
            EnableInput(true);
        }

        void clearUndoBuffer()
        {
            TextBase.IsUndoEnabled = false;
            TextBase.IsUndoEnabled = true;
        }

        public void WriteCustom(string data, Brush foreground, bool newline)
            => safeWrite(data, foreground, newline);

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
            => safeWrite(data, Config.ErrorFontBrush, true);

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

        string getInput()
            => CurrentLineString.Substring(currentPrompt.Text.Length);

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

        bool isAtOrBeforePrompt(TextPointer pointer)
            => (GetX(pointer) <= currentPrompt.Text.Length);

        int distanceToPromptEnd
            => GetX(TextBase.CaretPosition) - currentPrompt.Text.Length;
    }
}