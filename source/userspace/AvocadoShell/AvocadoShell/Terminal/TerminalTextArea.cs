using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService.Runspaces;
using AvocadoShell.Terminal.Modules;
using AvocadoUtilities.CommandLine.ANSI;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static AvocadoShell.Config;
using static StandardLibrary.Utilities.WPF;
using System.Windows.Controls;

namespace AvocadoShell.Terminal
{
    sealed class TerminalTextArea : InputTextArea, IShellUI
    {
        const DispatcherPriority OUTPUT_PRIORITY 
            = DispatcherPriority.Background;

        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();
        readonly Prompt currentPrompt = new Prompt();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly Task<PowerShellEngine> psEngineAsync;
        readonly Task<History> historyAsync;

        public TerminalTextArea()
        {
            psEngineAsync = Task.Run(() => new PowerShellEngine(this));
            historyAsync = Task.Run(createHistory);
            Task.Run(startPowershell);

            Unloaded += async (s, e) => await terminateExec();
            SizeChanged += onSizeChanged;

            addCommandBindings();
        }

        void addCommandBindings()
        {
            // Ctrl+A - Select input.
            this.BindCommand(ApplicationCommands.SelectAll, selectInput);

            // Disable PgUp/PgDn.
            new ICommand[] {
                EditingCommands.MoveDownByPage,
                EditingCommands.MoveUpByPage,
                EditingCommands.SelectDownByPage,
                EditingCommands.SelectUpByPage
            }.ForEach(c => this.BindCommand(c, () => { }));
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            ScrollToEnd();
        }

        async Task<History> createHistory() =>
            new History((await psEngineAsync).GetMaxHistoryCount());

        async Task startPowershell()
        {
            WriteErrorLine((await psEngineAsync).InitEnvironment());
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
            if (!(await psEngineAsync).Stop()) return;

            // Ensure the powershell thread is unblocked.
            if (!currentPrompt.FromShell) nonShellPromptDone.Signal(null);
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
            if (!atOrBeforePrompt()) return false;
            
            // The caret position does not change if text is selected
            // (unless Shift+Left is pressed) so we should not 
            // suppress that case.
            return Selection.IsEmpty || (key == Key.Left && IsShiftKeyDown);
        }

        bool handleHomeKey()
        {
            // Allow native handling if we are on separate line from the prompt
            // and Ctrl was not pressed.
            if (!IsControlKeyDown)
            {
                var lineStartOffset
                    = LineStartPointer.GetOffsetToPosition(getPromptPointer());
                if (lineStartOffset <= 0) return false;
            }

            // If we are on the same line as the prompt (or Ctrl is pressed), 
            // move the cursor to the end of the prompt instead of the beginning 
            // of the line.
            MoveCaret(getPromptPointer(), IsShiftKeyDown);
            return true;
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (Selection.IsEmpty) setInput(string.Empty);
            // Otherwise, cancel the selected text.
            else ClearSelection();
            return true;
        }

        bool handleUpAndDownKeys(Key key)
        {
            doInputManipulationWork(() => historyLookup(key == Key.Down));
            return true;
        }

        bool handleTabKey()
        {
            // Handle normally if not at the shell prompt.
            if (!currentPrompt.FromShell) return false;
            doInputManipulationWork(performAutocomplete);
            return true;
        }

        void doInputManipulationWork(Func<Task> work)
        {
            IsReadOnly = true;
            work().ContinueWith(
                t => IsReadOnly = false, 
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        bool handleEnterKey()
        {
            // Go to new line if shift is pressed at shell prompt.
            if (IsShiftKeyDown) return false;

            // Otherwise, execute the input.
            IsReadOnly = true;
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
        }

        void execute()
        {
            // Get user input.
            var input = getInput();

            // Position caret for writing command output.
            MoveCaret(EndPointer, false);
            WriteLine();

            // If this is the shell prompt, execute the input.
            if (currentPrompt.FromShell) Task.Run(() => executeInput(input));
            // Otherwise, signal that the we are done entering input.
            else nonShellPromptDone.Signal(input);
        }

        async Task executeInput(string input)
        {
            // Add command to history.
            (await historyAsync).Add(input);

            // Reset the buffer for command output.
            outputBuffer.Reset();

            var psEngine = await psEngineAsync;

            // Execute the command.
            WriteErrorLine(psEngine.ExecuteCommand(input));

            // Check if exit was requested.
            if (psEngine.ShouldExit) exit();
            // Otherwise, show the next shell prompt.
            else await writeShellPrompt();
        }

        void exit() => Dispatcher.BeginInvoke((Action)(
            () => Window.GetWindow(this).Close()));

        async Task performAutocomplete()
        { 
            // Get data needed for the completion.
            var input = getInput();
            var index = getInputTextRange(CaretPosition).Text.Length;
            var forward = !IsShiftKeyDown;

            // Perform the completion.
            var psEngine = await psEngineAsync;
            var hasCompletion = await Task.Run(
                () => psEngine.GetCompletion(ref input, ref index, forward));
            if (!hasCompletion) return;

            // Update the input (UI) with the result of the completion.
            setInput(input);
            MoveCaret(getPromptPointer().GetPositionAtOffset(index), false);
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
            return nonShellPromptDone.Block();
        }

        async Task writeShellPrompt() =>
            safeWritePromptCore(await getShellPromptString(), true, false);

        async Task<string> getShellPromptString()
        {
            var psEngine = await psEngineAsync;
            return Prompt.GetShellPromptString(
                psEngine.GetWorkingDirectory(),
                psEngine.RemoteComputerName);
        }

        void safeWritePromptCore(string prompt, bool fromShell, bool secure)
        {
            Dispatcher.BeginInvoke(
                (Action)(() => writePromptCore(prompt, fromShell, secure)),
                OUTPUT_PRIORITY);
        }

        void writePromptCore(string prompt, bool fromShell, bool secure)
        {
            if (fromShell)
            {
                // Update the window title to the shell prompt text.
                Window.GetWindow(this).Title =
                    $"{Prompt.ElevatedPrefix}{prompt}";

                // Write elevated prefix at shell prompt.
                if (!string.IsNullOrWhiteSpace(Prompt.ElevatedPrefix))
                    Write(Prompt.ElevatedPrefix, ElevatedBrush);
            }
            
            // Write prompt text.
            Write(prompt.TrimEnd(), PromptBrush);
            Write(" ", secure ? Brushes.Transparent : Foreground);
            
            ClearUndoBuffer();

            // Update the current prompt object.
            currentPrompt.Update(
                fromShell, StartPointer.GetOffsetToPosition(CaretPosition));

            // Enable user input.
            IsReadOnly = false;
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
            else safeWrite(data, SystemFontBrush, true);
        }

        public void WriteErrorLine(string data)
            => safeWrite(data, ErrorFontBrush, true);

        void safeWrite(string data, Brush foreground, bool newline)
        {
            Dispatcher.BeginInvoke(
                (Action<string, Brush, bool>)write,
                OUTPUT_PRIORITY,
                data, foreground, newline);
        }
        
        void write(string data, Brush foreground, bool newline)
        {
            // Run the data through the output buffer to determine if 
            // anything should be printed right now.
            if (!outputBuffer.ProcessNewOutput(ref data, ref foreground))
                return;

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
                        : SystemFontBrush;
                write(segment.Text, brush, newline);
            }),
            OUTPUT_PRIORITY);
        }

        async Task historyLookup(bool forward)
        {
            // Disallow lookup when not at the shell prompt.
            if (!currentPrompt.FromShell) return;

            var history = await historyAsync;

            // Cache the current user input to the buffer.
            history.CacheInput(getInput());

            // Look up the stored input to display from the buffer.
            var storedInput = history.Cycle(forward);

            // Return if no command was found.
            if (storedInput == null) return;

            // Update the display to show the new input.
            setInput(storedInput);
            MoveCaret(EndPointer, false);
        }

        string getInput() => getInputTextRange(EndPointer).Text;

        void setInput(string text) => getInputTextRange(EndPointer).Text = text;

        TextPointer getPromptPointer() 
            => StartPointer.GetPositionAtOffset(currentPrompt.LengthInSymbols);

        TextRange getInputTextRange(TextPointer end)
            => new TextRange(getPromptPointer(), end);
        
        bool atOrBeforePrompt()
            => string.IsNullOrEmpty(getInputTextRange(CaretPosition).Text);

        void selectInput() => Selection.Select(getPromptPointer(), EndPointer);
    }
}