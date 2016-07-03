using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.Engine.Modules;
using AvocadoShell.PowerShellService;
using AvocadoShell.PowerShellService.Runspaces;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
                => Dispatcher.BeginInvoke((Action)exit);
            return psEngine;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Scroll to the bottom when text changes.
            TextBase.TextChanged += (s, e) => TextBase.ScrollToEnd();

            psEngine.InitEnvironment();
        }
        
        async void onExecDone(object sender, ExecDoneEventArgs e)
        {
            // Display error, if any.
            if (!string.IsNullOrWhiteSpace(e.Error)) WriteErrorLine(e.Error);

            // Show the next shell prompt.
            await writeShellPrompt();
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

        bool handleBackAndLeftKeys(Key key)
        {
            if (TextBase.CaretPosition.Paragraph == null
                || !isAtOrBeforePrompt(TextBase.CaretPosition))
            {
                return false;
            }

            // The caret position does not change if text is selected
            // (unless Shift+Left is pressed) so we should not 
            // suppress that case.
            return TextBase.Selection.IsEmpty
                || (key == Key.Left && IsShiftKeyDown);
        }

        bool handleHomeKey()
        {
            // Allow native handling if we are on separate line from the prompt
            // and Ctrl was not pressed.
            if (!IsControlKeyDown)
            {
                var lineStart
                    = TextBase.CaretPosition.GetLineStartPosition(0);
                if (!isAtOrBeforePrompt(lineStart)) return false;
            }

            // If we are on the same line as the prompt, move the cursor
            // to the end of the prompt instead of the beginning of the
            // line.
            MoveCaret(-distanceToPromptEnd, IsShiftKeyDown);

            return true;
        }

        bool handleCtrlAKey()
        {
            if (!IsControlKeyDown) return false;

            // Ctrl+A will select all text after the prompt.
            MoveCaretToDocumentEnd();
            var promptPointer = TextBase.CaretPosition.GetPositionAtOffset(
                -distanceToPromptEnd);
            TextBase.Selection.Select(promptPointer, TextBase.CaretPosition);

            return true;
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (TextBase.Selection.IsEmpty) clearInput();
            // Otherwise, cancel the selected text.
            else ClearSelection();
            return true;
        }

        bool handlePageUpAndPageDownKeys()
        {
            // Currently no support for PageUp/PageDown.
            return true;
        }

        bool handleUpAndDownKeys(Key key)
        {
            inputHistoryLookup(key == Key.Down);
            return true;
        }

        bool handleTabKey()
        {
            performAutocomplete();
            return true;
        }

        bool handleEnterKey()
        {
            // Go to new line if shift is pressed at shell prompt.
            if (IsShiftKeyDown) return false;

            // Otherwise, execute the input.
            execute();
            return true;
        }

        protected override void HandleSpecialKeys(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    e.Handled = handleBackAndLeftKeys(e.Key);
                    break;
                case Key.Home:
                    e.Handled = handleHomeKey();
                    break;
                case Key.A:
                    e.Handled = handleCtrlAKey();
                    break;
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = handlePageUpAndPageDownKeys();
                    break;

                // Clear input or selection.
                case Key.Escape:
                    e.Handled = handleEscKey();
                    break;

                // Input history.
                case Key.Up:
                case Key.Down:
                    e.Handled = handleUpAndDownKeys(e.Key);
                    break;

                // Autocompletion.
                case Key.Tab:
                    e.Handled = handleTabKey();
                    break;

                // Handle command execution.
                case Key.Enter:
                    e.Handled = handleEnterKey();
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
            Window.GetWindow(this).Close();
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
            safeWritePromptCore(prompt, false, secure);
            return resetEvent.Block();
        }

        async Task writeShellPrompt()
            => safeWritePromptCore(await getShellPromptString(), true, false);

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

        void safeWritePromptCore(string prompt, bool fromShell, bool secure)
        {
            Dispatcher.BeginInvoke(
                (Action)(() => writePromptCore(prompt, fromShell, secure)),
                DispatcherPriority.Background);
        }

        void writePromptCore(string prompt, bool fromShell, bool secure)
        {
            // Update window title to prompt text, if this is the shell prompt.
            if (fromShell) Window.GetWindow(this).Title = prompt;

            // Write prompt text.
            var brush = fromShell ? Config.PromptBrush : Config.SystemFontBrush;
            Write(prompt.TrimEnd(), brush);
            Write(" ", secure ? Brushes.Transparent : Foreground);

            // Update the current prompt object.
            currentPrompt.Update(fromShell, FullText.Length);

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
                writeOutputLineWithANSICodes(data);
            }
            else safeWrite(data, Config.SystemFontBrush, true);
        }

        public void WriteErrorLine(string data)
            => safeWrite(data, Config.ErrorFontBrush, true);

        void safeWrite(string data, Brush foreground, bool newline)
        {
            Dispatcher.BeginInvoke(
                (Action<string, Brush, bool>)write,
                DispatcherPriority.Background,
                data, foreground, newline);
        }
        
        void write(string data, Brush foreground, bool newline)
        {
            var action = newline
                ? (Action<string, Brush>)WriteLine
                : (Action<string, Brush>)Write;
            action(data, foreground);
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

        void writeANSISegment(ANSISegment segment, bool newline)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var brush = segment.Color.HasValue
                        ? new SolidColorBrush(segment.Color.Value)
                        : Config.SystemFontBrush;
                write(segment.Text, brush, newline);
            }),
            DispatcherPriority.Background);
        }

        string getInput() => FullText.Substring(currentPrompt.Length);

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
            => (GetX(pointer) <= currentPrompt.Length);

        int distanceToPromptEnd
            => GetX(TextBase.CaretPosition) - currentPrompt.Length;
    }
}