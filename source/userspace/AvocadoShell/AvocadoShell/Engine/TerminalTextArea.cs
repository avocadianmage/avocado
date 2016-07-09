﻿using AvocadoFramework.Controls.TextRendering;
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
        readonly InputHistory inputHistory = new InputHistory();
        readonly ResetEventWithData<string> resetEvent
            = new ResetEventWithData<string>();
        readonly Prompt currentPrompt = new Prompt();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly Task<PowerShellEngine> psEngineAsync;

        public TerminalTextArea()
        {
            psEngineAsync = Task.Run(() => createPowerShellEngine());

            Unloaded += async (s, e) => await terminateExec();
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

            // Prepare the Powershell engine for user interaction.
            Task.Run(async () => (await psEngineAsync).InitEnvironment());

            TextBase.TextChanged += (s, e) => TextBase.ScrollToEnd();
            TextBase.SizeChanged += onSizeChanged;
        }
        
        async void onExecDone(object sender, ExecDoneEventArgs e)
        {
            // Display error, if any.
            if (!string.IsNullOrWhiteSpace(e.Error)) WriteErrorLine(e.Error);

            // Show the next shell prompt.
            await writeShellPrompt();
        }

        async void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;

            // Set the character buffer width of the PowerShell host.
            var bufferWidth =
                (int)Math.Ceiling(e.NewSize.Width / CharDimensions.Width);
            (await psEngineAsync).HostRawUI.BufferSize
                = new System.Management.Automation.Host.Size(bufferWidth, 1);
        }

        async Task terminateExec()
        {
            // Terminate the powershell process.
            (await psEngineAsync).Stop();

            // Ensure the powershell thread is unblocked.
            resetEvent.Signal(null);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Always detect Ctrl+B break.
            if (IsControlKeyDown && e.Key == Key.B)
            {
                Task.Run(terminateExec);
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        bool handleBackAndLeftKeys(Key key)
        {
            if (!atOrBeforePrompt(CaretPointer)) return false;

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
            if (!IsControlKeyDown && !atOrBeforePrompt(LineStartPointer))
            {
                return false;
            }

            // If we are on the same line as the prompt (or Ctrl is pressed), 
            // move the cursor to the end of the prompt instead of the beginning 
            // of the line.
            MoveCaret(getPromptPointer(), IsShiftKeyDown);

            return true;
        }

        bool handleCtrlAKey()
        {
            if (!IsControlKeyDown) return false;

            // Ctrl+A will select all text after the prompt.
            TextBase.Selection.Select(getPromptPointer(), EndPointer);

            return true;
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (TextBase.Selection.IsEmpty) setInput(string.Empty);
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
            performAutocomplete().RunAsync();
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
            MoveCaret(EndPointer, false);
            WriteLine();

            // Signal to the powershell process that the we are done entering
            // input.
            resetEvent.Signal(input);

            // Quit if the input was entered due to a custom prompt in an 
            // executing process.
            if (!currentPrompt.FromShell) return;

            Task.Run(() => executeCommand(input));
        }

        async Task executeCommand(string input)
        {
            // Add command to history.
            inputHistory.Add(input);

            // Otherwise, use PowerShell to interpret the command.
            (await psEngineAsync).ExecuteCommand(input);
        }
        
        public async Task<bool> RunNativeCommand(string message)
            => (await psEngineAsync).RunNativeCommand(message);

        async Task performAutocomplete()
        {
            EnableInput(false);

            var input = getInput();
            var index = getInputTextRange(CaretPointer).Text.Length;
            var forward = !IsShiftKeyDown;

            var psEngine = await psEngineAsync;
            var completion = await Task.Run(
                () => psEngine.GetCompletion(input, index, forward));
            if (completion != null) setInput(completion);

            EnableInput(true);
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
            var psEngine = await psEngineAsync;
            if (psEngine.RemoteComputerName != null)
            {
                prompt += $"[{psEngine.RemoteComputerName}] ";
            }

            // Add working directory.
            prompt += $"{psEngine.GetWorkingDirectory()} ";

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
            // Check if this is the shell prompt.
            if (fromShell)
            {
                outputBuffer.Reset();

                // Update the window title to the prompt text.
                Window.GetWindow(this).Title = prompt;
            }

            // Write prompt text.
            var brush = fromShell ? Config.PromptBrush : Config.SystemFontBrush;
            Write(prompt.TrimEnd(), OutputBuffer.CreateTextFadeBrush(brush));
            Write(" ", secure ? Brushes.Transparent : Foreground);
            
            clearUndoBuffer();

            // Update the current prompt object.
            currentPrompt.Update(
                fromShell, StartPointer.GetOffsetToPosition(CaretPointer));
            
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
            // Run the data through the output buffer to determine if 
            // anything should be printed right now.
            if (!outputBuffer.ProcessNewOutput(ref data, ref foreground))
            {
                return;
            }

            var action = newline ? (Action<string, Brush>)WriteLine : Write;
            action(data, foreground);
        }

        void writeOutputLineWithANSICodes(string data)
        {
            var segments = ANSICode.GetColorSegments(data);
            if (!segments.Any()) return;
            segments
                .Take(segments.Count() - 1)
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
            setInput(storedInput);
        }

        string getInput() => getInputTextRange(EndPointer).Text;

        void setInput(string text)
        {
            getInputTextRange(EndPointer).Text = text;
            MoveCaret(EndPointer, false);
        }

        TextPointer getPromptPointer() 
            => StartPointer.GetPositionAtOffset(currentPrompt.LengthInSymbols);

        TextRange getInputTextRange(TextPointer end)
            => new TextRange(getPromptPointer(), end);

        bool atOrBeforePrompt(TextPointer pointer)
        {
            return StartPointer.GetOffsetToPosition(pointer)
                <= currentPrompt.LengthInSymbols;
        }
    }
}