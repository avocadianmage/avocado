using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService;
using AvocadoUtilities.CommandLine.ANSI;
using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static AvocadoShell.Config;

namespace AvocadoShell.UI.Terminal
{
    sealed class TerminalEditor : AvocadoTextEditor, IShellUI
    {
        readonly PowerShellEngine engine = new PowerShellEngine();
        readonly Prompt currentPrompt = new Prompt();
        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();
        readonly OutputBuffer outputBuffer = new OutputBuffer();

        public TerminalEditor()
        {
            subscribeToEvents();
            loadSyntaxHighlighting();
            addCommandBindings();
            disableUndo();
        }

        void subscribeToEvents()
        {
            Loaded += onLoaded;
            Unloaded += (s, e) => terminateExec();
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            var lineBufferLength = getLineBufferLength();
            Task.Run(() => startCommandline(lineBufferLength));

            SizeChanged += onSizeChanged;
        }

        void disableUndo() => Document.UndoStack.SizeLimit = 0;

        void addCommandBindings()
        {
            var caretNavigationCommandBindings
                = TextArea.DefaultInputHandler.CaretNavigation.CommandBindings;
            caretNavigationCommandBindings.AddNewBinding(
                ApplicationCommands.SelectAll, selectInput);

            // 'Home' commands.
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveToDocumentStart, 
                () => moveCaretToPromptStart(false));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.SelectToDocumentStart,
                () => moveCaretToPromptStart(true));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveToLineStart, 
                () => moveCaretToLineStart(false));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.SelectToLineStart, 
                () => moveCaretToLineStart(true));

            TextArea.DefaultInputHandler.Editing.CommandBindings
                .AddNewBinding(ApplicationCommands.Cut, cut);
        }

        void selectInput()
        {
            if (IsReadOnly) return;
            Select(currentPrompt.EndOffset, Document.TextLength);
        }

        void moveCaretToPromptStart(bool select)
        {
            moveCaretAsInput(currentPrompt.EndOffset, select);
        }

        void moveCaretToLineStart(bool select)
        {
            var endOffset = isCaretOnPromptLine
                ? currentPrompt.EndOffset
                : Document.GetLineByOffset(CaretOffset).Offset;
            moveCaretAsInput(endOffset, select);
        }

        void moveCaretAsInput(int endOffset, bool select)
        {
            if (IsReadOnly) return;

            if (select) Select(SelectionStart, endOffset);
            else CaretOffset = endOffset;
        }

        void cut()
        {
            if (IsReadOnly) return;

            if (TextArea.Selection.IsEmpty)
            {
                var line = Document.GetLineByOffset(CaretOffset);
                var offset = isCaretOnPromptLine
                    ? currentPrompt.EndOffset : line.Offset;
                Select(offset, line.EndOffset);
            }

            Clipboard.SetText(SelectedText);
            SelectedText = string.Empty;
        }

        void terminateExec()
        {
            // Terminate the powershell process.
            if (!engine.MyHost.CurrentPipeline.Stop()) return;

            // Ensure the powershell thread is unblocked.
            if (!currentPrompt.FromShell) nonShellPromptDone.Signal(null);
        }

        void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            engine.MyHost.UpdateBufferWidth(getLineBufferLength());
        }

        int getLineBufferLength()
        {
            var maxBufferLength = (int)ActualWidth;
            Document.Insert(0, new string(' ', maxBufferLength));
            var textPosition = GetPositionFromPoint(new Point(ActualWidth, 0));
            Document.Remove(0, maxBufferLength);
            return textPosition.Value.Column;
        }

        void loadSyntaxHighlighting()
        {
            const string XHSD
                = "AvocadoShell.PowerShellAssets.Formatting.AvocadoPowerShell.xshd";
            SetSyntaxHighlighting(XHSD);
        }

        void startCommandline(int lineBufferLength)
        {
            engine.Initialize(this, lineBufferLength);
            writeShellPrompt();
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

        void writeShellPrompt() => safeWritePromptCore("> ", true, false);

        void safeWritePromptCore(string prompt, bool fromShell, bool secure)
            => Dispatcher.InvokeAsync(
                () => writePromptCore(prompt, fromShell, secure), TextPriority);

        void writePromptCore(string prompt, bool fromShell, bool secure)
        {
            currentPrompt.FromShell = fromShell;

            // Write prompt text.
            if (fromShell) setShellTitle();
            Write(prompt, EnvUtils.IsAdmin ? ElevatedPromptBrush : PromptBrush); 
            //TODO: turn prompt into timestamp

            // Enable user input.
            currentPrompt.EndOffset = CaretOffset;
            IsReadOnly = false;
        }

        void setShellTitle()
        {
            var title = Prompt.GetShellTitleString(
                engine.GetWorkingDirectory(), engine.RemoteComputerName);

            // Only write shell title if it has changed.
            if (title == currentPrompt.ShellTitle) return;

            // Update prompt object with new shell title.
            currentPrompt.ShellTitle = title;

            // Update the window title to the shell title text.
            Window.GetWindow(this).Title = title;

            // Separate title from other output with a newline beforehand.
            if (CaretOffset > 0) WriteLine();
            WriteLine(title, VerboseBrush);
        }

        public void WriteCustom(string text, Brush foreground, bool newline)
            => safeWrite(text, foreground, newline);

        public void WriteOutputLine(string text)
            => safeWrite(text, OutputBrush, true);

        public void WriteErrorLine(string text)
            => safeWrite(text, ErrorBrush, true);

        void safeWrite(string text, Brush foreground, bool newline)
            => Dispatcher.InvokeAsync(
                () => write(text, foreground, newline), TextPriority);

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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Break out of the current execution.
                case Key.B:
                    if (WPFUtils.IsControlKeyDown)
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

                // Clear input or selection.
                case Key.Escape:
                    e.Handled = IsReadOnly || handleEscKey();
                    break;

                // Autocompletion.
                case Key.Tab:
                    e.Handled = IsReadOnly || handleTabKey();
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

                // Command execution.
                case Key.Enter:
                    e.Handled = IsReadOnly || handleEnterKey();
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        bool handleBackAndLeftKeys(Key key)
        {
            if (!isCaretAtOrBeforePromptEnd()) return false;

            // The caret position does not change if text is selected
            // (unless Shift+Left is pressed) so we should not 
            // suppress that case.
            return TextArea.Selection.IsEmpty 
                || (key == Key.Left && WPFUtils.IsShiftKeyDown);
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (TextArea.Selection.IsEmpty)
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
                performAutocomplete(!WPFUtils.IsShiftKeyDown).RunAsync();
            }
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

        bool handleEnterKey()
        {
            // Go to new line if shift is pressed at shell prompt.
            if (WPFUtils.IsShiftKeyDown) return false;

            // Otherwise, execute the input.
            IsReadOnly = true;
            execute();
            return true;
        }

        bool isCaretOnPromptLine
        {
            get
            {
                var promptLine = Document
                    .GetLineByOffset(currentPrompt.EndOffset).LineNumber;
                return TextArea.Caret.Line == promptLine;
            }
        }

        bool isCaretAtOrBeforePromptEnd() 
            => CaretOffset <= currentPrompt.EndOffset;

        int inputLength => Document.TextLength - currentPrompt.EndOffset;

        string getInput() 
            => Document.GetText(currentPrompt.EndOffset, inputLength);

        void setInput(string text)
            => Document.Replace(currentPrompt.EndOffset, inputLength, text);
        
        async Task performAutocomplete(bool forward)
        {
            // Get data needed for the completion lookup.
            var input = getInput();
            var index = CaretOffset - currentPrompt.EndOffset;

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

            // Update the input (UI) with the result of the completion.
            Document.Replace(
                replacementIndex + currentPrompt.EndOffset,
                replacementLength,
                completionText);
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
            CaretOffset = Document.TextLength;
        }

        void scrollPage(bool down)
        {
            ScrollToVerticalOffset(
                VerticalOffset + ViewportHeight * (down ? 1 : -1));
        }

        void execute()
        {
            // Get user input.
            var input = getInput();

            // Position caret for writing command output.
            TextArea.ClearSelection();
            CaretOffset = Document.TextLength;
            WriteLine();

            // If this is the shell prompt, execute the input.
            if (currentPrompt.FromShell) Task.Run(() => executeInput(input));
            // Otherwise, signal that the we are done entering input.
            else nonShellPromptDone.Signal(input);
        }

        void executeInput(string input)
        {
            outputBuffer.Reset();

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
    }
}
