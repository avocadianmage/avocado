using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService;
using AvocadoUtilities.CommandLine.ANSI;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static AvocadoShell.Config;
using static StandardLibrary.Utilities.WPF;

namespace AvocadoShell.Terminal
{
    sealed class TerminalTextArea : InputTextArea, IShellUI
    {
        const DispatcherPriority TEXT_PRIORITY 
            = DispatcherPriority.ContextIdle;

        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();
        readonly Prompt currentPrompt = new Prompt();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly PowerShellEngine engine = new PowerShellEngine();
        readonly PSSyntaxHighlighter highlighter = new PSSyntaxHighlighter();

        public TerminalTextArea()
        {
            Task.Run(() => startCommandline());

            Unloaded += (s, e) => terminateExec();
            SizeChanged += onSizeChanged;
            TextChanged += onTextChanged;

            addCommandBindings();
        }

        void startCommandline()
        {
            engine.Initialize(this);
            writeShellPrompt();
        }

        void addCommandBindings()
        {
            // Ctrl+A - Select input.
            this.BindCommand(ApplicationCommands.SelectAll, selectInput);

            // Up/Down - Access command history.
            this.BindCommand(
                EditingCommands.MoveUpByLine, 
                () => setInputFromHistory(false));
            this.BindCommand(
                EditingCommands.MoveDownByLine,
                () => setInputFromHistory(true));

            // PgUp/PgDn - scroll a page at a time without moving the text 
            // cursor.
            this.BindCommand(
                EditingCommands.MoveDownByPage, () => scrollPage(true));
            this.BindCommand(
                EditingCommands.MoveUpByPage, () => scrollPage(false));

            // Disable Shift+PgUp/PgDn.
            new ICommand[] {
                EditingCommands.SelectDownByPage,
                EditingCommands.SelectUpByPage
            }.ForEach(c => this.BindCommand(c, () => { }));
        }

        void scrollPage(bool down)
        {
            var direction = down ? 1 : -1;
            ScrollToVerticalOffset(VerticalOffset + ViewportHeight * direction);
        }

        async void onTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReadOnly || !currentPrompt.FromShell) return;
            await performSyntaxHighlighting();
        }

        async Task performSyntaxHighlighting()
        {
            var input = getInput();
            var tokenization = await Task.Run(
                () => highlighter.GetChangedTokens(input));
            var promptStart = getPromptPointer();
            tokenization.ForEach(p =>
            {
                Dispatcher.InvokeAsync(
                    () => applyTokenColoring(promptStart, p.Key, p.Value),
                    TEXT_PRIORITY);
            });
        }

        void applyTokenColoring(
            TextPointer promptStart, PSToken token, Color? color)
        {
            var start = GetPointerFromCharOffset(promptStart, token.Start);
            if (start == null) return;
            var end = GetPointerFromCharOffset(start, token.Length);
            if (end == null) return;

            var foreground = color.HasValue
                ? new SolidColorBrush(color.Value) : Foreground;
            new TextRange(start, end).ApplyPropertyValue(
                TextElement.ForegroundProperty, foreground);
        }

        void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            
            // Set the character buffer width of the PowerShell host.
            var bufferWidth = (int)Math.Ceiling(
                e.NewSize.Width / CharDimensions.Width);
            engine.MyHost.UI.RawUI.BufferSize
                = new System.Management.Automation.Host.Size(bufferWidth, 1);
        }

        void terminateExec()
        {
            // Terminate the powershell process.
            if (!engine.MyHost.CurrentPipeline.Stop()) return;

            // Ensure the powershell thread is unblocked.
            if (!currentPrompt.FromShell) nonShellPromptDone.Signal(null);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Always detect Ctrl+B break.
            if (IsControlKeyDown && e.Key == Key.B)
            {
                terminateExec();
                return;
            }

            // Handle other special keys if input is allowed.
            if (!IsReadOnly) handleSpecialKeys(e);

            base.OnPreviewKeyDown(e);
        }

        void handleSpecialKeys(KeyEventArgs e)
        {
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

        bool handleTabKey()
        {
            // Handle normally if not at the shell prompt.
            if (!currentPrompt.FromShell) return false;
            performAutocomplete(!IsShiftKeyDown).RunAsync();
            return true;
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

        protected override void OnPaste()
        {
            highlighter.Reset();
            base.OnPaste();
        }

        void execute()
        {
            // Get user input.
            var input = getInput();

            // Position caret for writing command output.
            MoveCaret(EndPointer, false);
            CaretPosition.InsertTextInRun(" ");
            WriteLine();

            // If this is the shell prompt, execute the input.
            if (currentPrompt.FromShell) Task.Run(() => executeInput(input));
            // Otherwise, signal that the we are done entering input.
            else nonShellPromptDone.Signal(input);
        }

        void executeInput(string input)
        {
            outputBuffer.Reset();
            highlighter.Reset();

            // Execute the command.
            var error = engine.ExecuteCommand(input);
            if (error != null) WriteErrorLine(error);

            // Check if exit was requested.
            if (engine.MyHost.ShouldExit) exit();
            // Otherwise, show the next shell prompt.
            else writeShellPrompt();
        }

        void exit() 
            => Dispatcher.InvokeAsync(() => Window.GetWindow(this).Close());

        async Task performAutocomplete(bool forward)
        { 
            // Get data needed for the completion.
            var input = getInput();
            var index = getInputTextRange(CaretPosition).Text.Length;

            // Perform the completion.
            var hasCompletion = await Task.Run(
                () => engine.MyHost.CurrentPipeline.Autocomplete.GetCompletion(
                    ref input, ref index, forward));
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

        void writeShellPrompt()
            => safeWritePromptCore(getShellPromptString(), true, false);

        string getShellPromptString()
        {
            return Prompt.GetShellPromptString(
                engine.GetWorkingDirectory(),
                engine.RemoteComputerName);
        }

        void safeWritePromptCore(string prompt, bool fromShell, bool secure)
            => Dispatcher.InvokeAsync(
                () => writePromptCore(prompt, fromShell, secure),
                TEXT_PRIORITY);

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
            var run = Write(" ", secure ? Brushes.Transparent : Foreground);
            if (!secure) run.Background = InputBackgroundBrush;
            
            // Update the current prompt object.
            currentPrompt.Update(
                fromShell, StartPointer.GetOffsetToPosition(CaretPosition));

            // Enable user input.
            IsReadOnly = false;
        }

        public void WriteCustom(string text, Brush foreground, bool newline)
            => safeWrite(text, foreground, newline);

        public void WriteOutputLine(string text)
        {
            // Check if the line contains any ANSI codes that we should process.
            if (ANSICode.ContainsANSICodes(text))
            {
                writeOutputLineWithANSICodes(text);
            }
            else safeWrite(text, SystemFontBrush, true);
        }

        public void WriteErrorLine(string text)
            => safeWrite(text, ErrorFontBrush, true);

        void safeWrite(string text, Brush foreground, bool newline)
            => Dispatcher.InvokeAsync(
                () => write(text, foreground, newline), TEXT_PRIORITY);
        
        void write(string text, Brush foreground, bool newline)
        {
            // Run the data through the output buffer to determine if 
            // anything should be printed right now.
            if (!outputBuffer.ProcessNewOutput(ref text, newline)) return;

            if (newline) WriteLine(text, foreground);
            else Write(text, foreground);
        }

        void writeOutputLineWithANSICodes(string text)
        {
            var segments = ANSICode.GetColorSegments(text);
            if (!segments.Any()) return;
            segments
                .Take(segments.Count() - 1)
                .ForEach(seg => writeANSISegment(seg, false));
            writeANSISegment(segments.Last(), true);
        }

        void writeANSISegment(ANSISegment segment, bool newline)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var brush = segment.Color.HasValue
                        ? new SolidColorBrush(segment.Color.Value)
                        : SystemFontBrush;
                write(segment.Text, brush, newline);
            },
            TEXT_PRIORITY);
        }

        void setInputFromHistory(bool forward)
        {
            // Disallow lookup when not at the shell prompt.
            if (!currentPrompt.FromShell) return;

            // Cache the current user input to the buffer and look up the stored 
            // input to display from the buffer.
            var storedInput = engine.MyHistory.Cycle(getInput(), forward);

            // Return if no command was found.
            if (storedInput == null) return;

            // Update the display to show the new input.
            setInput(storedInput);
            MoveCaret(EndPointer, false);
        }

        string getInput() => getInputTextRange(EndPointer).Text;

        void setInput(string text)
        {
            highlighter.Reset();
            getInputTextRange(EndPointer).Text = text;
        }

        TextPointer getPromptPointer() 
            => StartPointer.GetPositionAtOffset(currentPrompt.LengthInSymbols);

        TextRange getInputTextRange(TextPointer end)
            => new TextRange(getPromptPointer(), end);
        
        bool atOrBeforePrompt()
            => string.IsNullOrEmpty(getInputTextRange(CaretPosition).Text);

        void selectInput() => Selection.Select(getPromptPointer(), EndPointer);
    }
}