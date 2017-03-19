using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.Interfaces;
using AvocadoShell.PowerShellService;
using AvocadoShell.UI.Utilities;
using AvocadoUtilities.CommandLine.ANSI;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static AvocadoShell.Config;
using static StandardLibrary.WPF.WPFUtils;

namespace AvocadoShell.UI.Terminal
{
    sealed class TerminalTextArea : InputTextArea, IShellUI
    {
        const DispatcherPriority TEXT_PRIORITY = DispatcherPriority.ContextIdle;

        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();
        readonly Prompt currentPrompt = new Prompt();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly PowerShellEngine engine = new PowerShellEngine();
        readonly PSSyntaxHighlighter highlighter = new PSSyntaxHighlighter();

        public TerminalTextArea()
        {
            Task.Run(() => startCommandline());
            subscribeToEvents();
            addCommandBindings();
        }

        void startCommandline()
        {
            engine.Initialize(this);
            writeShellPrompt();
        }

        void subscribeToEvents()
        {
            Unloaded += (s, e) => terminateExec();
            SizeChanged += onSizeChanged;
            TextChanged += onTextChanged;
        }

        void addCommandBindings()
        {
            // Ctrl+A - Select input.
            this.BindCommand(ApplicationCommands.SelectAll, selectInput);
        }

        void scrollPage(bool down)
        {
            ScrollToVerticalOffset(
                VerticalOffset + ViewportHeight * (down ? 1 : -1));
        }

        async void onTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReadOnly || !currentPrompt.FromShell) return;
            await highlighter.Highlight(this, getInputTextRange(EndPointer));
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

        protected override void HandleSpecialKeys(KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Break out of the current execution.
                case Key.B:
                    if (IsControlKeyDown)
                    {
                        terminateExec();
                        e.Handled = true;
                    }
                    break;

                // Prevent overwriting the prompt.
                case Key.Back:
                case Key.Left:
                    e.Handled = IsReadOnly || handleBackAndLeftKeys(e.Key);
                    break;
                case Key.Home:
                    e.Handled = IsReadOnly || handleHomeKey();
                    break;

                // Clear input or selection.
                case Key.Escape:
                    e.Handled = IsReadOnly || handleEscKey();
                    break;

                // Autocompletion.
                case Key.Tab:
                    e.Handled = IsReadOnly || handleTabKey();
                    break;

                // Command execution.
                case Key.Enter:
                    e.Handled = IsReadOnly || handleEnterKey();
                    break;

                // History.
                case Key.Up:
                case Key.Down:
                    e.Handled = IsReadOnly || handleUpDown(e.Key);
                    break;

                // Scroll page up/down (can be used while readonly).
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = handlePageUpDown(e.Key);
                    break;
            }

            if (!e.Handled) base.HandleSpecialKeys(e);
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
            // move the cursor to the end of the prompt.
            if (IsShiftKeyDown)
            {
                EditingCommands.SelectToDocumentStart.Execute(null, this);
                Selection.Select(Selection.End, getPromptPointer());
            }
            else CaretPosition = getPromptPointer();
            return true;
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (Selection.IsEmpty)
            {
                setInput(string.Empty);
                return true;
            }
            return false;
        }

        bool handleTabKey()
        {
            // Perform autocomplete if at the shell prompt.
            if (currentPrompt.FromShell)
            {
                performAutocomplete(!IsShiftKeyDown).RunAsync();
                return true;
            }
            return false;
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

        bool handleUpDown(Key key)
        {
            setInputFromHistory(key == Key.Down);
            return true;
        }

        bool handlePageUpDown(Key key)
        {
            scrollPage(key == Key.PageDown);
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
            CaretPosition = EndPointer;
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
            // Get data needed for the completion lookup.
            var input = getInput();
            var index = getInputTextRange(CaretPosition).Text.Length;

            // Retrieve the completion text.
            var replacementIndex = default(int);
            var replacementLength = default(int);
            var completionText = default(string);
            IsReadOnly = true;
            var hasCompletion = await Task.Run(
                () => engine.MyHost.CurrentPipeline.Autocomplete.GetCompletion(
                    input, index, forward, 
                    out replacementIndex, 
                    out replacementLength,
                    out completionText));
            IsReadOnly = false;
            if (!hasCompletion) return;

            // Quit if the completion matches the text that is already 
            // displayed.
            var range = getCompletionReplacementRange(
                replacementIndex, replacementLength);
            if (range.Text == completionText) return;

            // Update the input (UI) with the result of the completion.
            range.Text = completionText;
            CaretPosition = range.End;
        }

        TextRange getCompletionReplacementRange(
            int replacementIndex, int replacementLength)
        {
            var replacementStart = getPromptPointer().GetPointerFromCharOffset(
                replacementIndex);
            var replacementEnd = replacementStart.GetPointerFromCharOffset(
                replacementLength);
            return new TextRange(replacementStart, replacementEnd);
        }

        public string WritePrompt(string prompt) => writePrompt(prompt, false);

        public SecureString WriteSecurePrompt(string prompt)
        {
            var secureStr = new SecureString();
            writePrompt(prompt, true).ForEach(secureStr.AppendChar);
            secureStr.MakeReadOnly();
            return secureStr;
        }

        string writePrompt(string prompt, bool secure)
        {
            safeWritePromptCore(prompt, false, secure);
            return nonShellPromptDone.Block();
        }

        void writeShellPrompt()
        {
            var promptStr = Prompt.GetShellPromptString(
                engine.GetWorkingDirectory(), engine.RemoteComputerName);
            safeWritePromptCore(promptStr, true, false);
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
            Write(" ", secure ? Brushes.Transparent : Foreground);
            
            // Update the current prompt object.
            currentPrompt.Update(
                fromShell, StartPointer.GetOffsetToPosition(CaretPosition));

            // Enable user input.
            IsReadOnly = false;
        }

        public void WriteCustom(string text, Brush foreground, bool newline)
            => safeWrite(text, foreground, newline);

        public void WriteOutputLine(string text) 
            => safeWrite(text, OutputBrush, true);

        public void WriteErrorLine(string text)
            => safeWrite(text, ErrorBrush, true);

        void safeWrite(string text, Brush foreground, bool newline)
            => Dispatcher.InvokeAsync(
                () => write(text, foreground, newline), TEXT_PRIORITY);
        
        void write(string text, Brush foreground, bool newline)
        {
            // Run the data through the output buffer to determine if 
            // anything should be printed right now.
            if (!outputBuffer.ProcessNewOutput(ref text, newline)) return;

            // Check if the line contains any ANSI codes that we should process.
            if (ANSICode.ContainsANSICodes(text))
            {
                writeOutputLineWithANSICodes(text);
                return;
            }

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
            var brush = segment.Color.HasValue
                ? new SolidColorBrush(segment.Color.Value) : OutputBrush;
            write(segment.Text, brush, newline);
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
            CaretPosition = EndPointer;
        }

        TextPointer StartPointer => Document.ContentStart;

        TextPointer EndPointer =>
           Document.ContentEnd
               // Omit default newline that is at the end of the document.
               .GetNextInsertionPosition(LogicalDirection.Backward);

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

        void selectInput()
        {
            if (IsReadOnly) return;
            Selection.Select(getPromptPointer(), EndPointer);
        }

        public void EditFile(string path)
        {
            Dispatcher.InvokeAsync(
                () => ((MainWindow)Window.GetWindow(this)).OpenEditor(path));
        }
    }
}