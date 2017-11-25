using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.PowerShellService;
using AvocadoUtilities.CommandLine.ANSI;
using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static AvocadoShell.Config;

namespace AvocadoShell.UI
{
    sealed class TerminalEditor : AvocadoTextEditor, IShellUI
    {
        readonly PowerShellEngine engine = new PowerShellEngine();
        readonly Prompt prompt = new Prompt();
        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly TerminalReadOnlySectionProvider readOnlyProvider
            = new TerminalReadOnlySectionProvider();

        public TerminalEditor()
        {
            // Set up readonly handling for prompts.
            TextArea.ReadOnlySectionProvider = readOnlyProvider;

            // Disable hyperlinks.
            TextArea.Options.EnableEmailHyperlinks = false;
            TextArea.Options.EnableHyperlinks = false;

            // Subscribe to events.
            Loaded += onLoaded;
            Unloaded += (s, e) => terminateExec();

            disableChangingText();
            loadSyntaxHighlighting();
            addCommandBindings();
        }

        void enableChangingText()
        {
            readOnlyProvider.IsReadOnly = false;
            Document.UndoStack.SizeLimit = int.MaxValue;
        }

        void disableChangingText()
        {
            readOnlyProvider.IsReadOnly = true;
            Document.UndoStack.SizeLimit = 0;
        }

        int readOnlyEndOffset
        {
            get
            {
                return readOnlyProvider.IsReadOnly
                    ? Document.TextLength : readOnlyProvider.PromptEndOffset;
            }
        }

        bool isCaretOnPromptLine
        {
            get
            {
                var promptLine = Document
                    .GetLineByOffset(readOnlyProvider.PromptEndOffset)
                    .LineNumber;
                return TextArea.Caret.Line == promptLine;
            }
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            var lineBufferLength = getLineBufferLength();
            Task.Run(() => startCommandline(lineBufferLength));

            SizeChanged += onSizeChanged;
        }

        void addCommandBindings()
        {
            var caretNavigationCommandBindings
                = TextArea.DefaultInputHandler.CaretNavigation.CommandBindings;

            // Select input.
            caretNavigationCommandBindings
                .AddNewBinding(ApplicationCommands.SelectAll, selectInput);

            // 'Home' commands.
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveToDocumentStart,
                () => moveCaretToDocumentStart(false));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.SelectToDocumentStart,
                () => moveCaretToDocumentStart(true));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveToLineStart,
                () => moveCaretToLineStart(false));
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.SelectToLineStart,
                () => moveCaretToLineStart(true));

            // Page Up and Page Down commands.
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveDownByPage, () => { });
            caretNavigationCommandBindings.AddNewBinding(
                EditingCommands.MoveUpByPage, () => { });

            // Execute input.
            TextArea.DefaultInputHandler.Editing.CommandBindings
                .AddNewBinding(EditingCommands.EnterParagraphBreak, execute);

            // Execution break.
            TextArea.DefaultInputHandler.AddBinding(
                new RoutedCommand(),
                ModifierKeys.Control,
                Key.B,
                (s, e) => terminateExec());
        }

        void selectInput()
        {
            if (readOnlyProvider.IsReadOnly) return;
            Select(readOnlyProvider.PromptEndOffset, Document.TextLength);
            TextArea.Caret.BringCaretToView();
        }

        void moveCaretToDocumentStart(bool select)
        {
            var targetOffset 
                = CaretOffset < readOnlyEndOffset ? 0 : readOnlyEndOffset;
            moveCaret(targetOffset, select);
        }

        void moveCaretToLineStart(bool select)
        {
            var caretLineStartOffset 
                = Document.GetLineByOffset(CaretOffset).Offset;
            var targetOffset = isCaretOnPromptLine
                ? (CaretOffset < readOnlyEndOffset 
                    ? caretLineStartOffset 
                    : readOnlyEndOffset)
                : caretLineStartOffset;
            moveCaret(targetOffset, select);
        }

        void moveCaret(int offset, bool select)
        {
            if (IsReadOnly) return;

            if (select) Select(SelectionStart, offset);
            else CaretOffset = offset;

            TextArea.Caret.BringCaretToView();
        }

        void terminateExec()
        {
            // Terminate the powershell process.
            if (!engine.MyHost.CurrentPipeline.Stop()) return;

            // Ensure the powershell thread is unblocked.
            if (!prompt.FromShell) nonShellPromptDone.Signal(null);
        }

        void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            engine.MyHost.UpdateBufferWidth(getLineBufferLength());
        }

        int getLineBufferLength()
        {
            return (int)(ActualWidth / TextArea.TextView.WideSpaceWidth);
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

        public string WritePrompt(string text) => writePrompt(text, false);

        public SecureString WriteSecurePrompt(string text)
        {
            var secureStr = new SecureString();
            writePrompt(text, true).ForEach(secureStr.AppendChar);
            secureStr.MakeReadOnly();
            return secureStr;
        }

        string writePrompt(string text, bool secure)
        {
            safeWritePromptCore(text, false, secure);
            return nonShellPromptDone.Block();
        }

        void writeShellPrompt()
        {
            safeWritePromptCore(Prompt.GetShellPromptString, true, false);
        }

        void safeWritePromptCore(string text, bool fromShell, bool secure)
            => Dispatcher.InvokeAsync(
                () => writePromptCore(text, fromShell, secure), TextPriority);

        void writePromptCore(string text, bool fromShell, bool secure)
        {
            prompt.FromShell = fromShell;

            if (fromShell) updateWorkingDirectory();

            // Write prompt text.
            Append(text, EnvUtils.IsAdmin ? ElevatedPromptBrush : PromptBrush);

            // Enable user input.
            readOnlyProvider.PromptEndOffset = Document.TextLength;
            enableChangingText();
        }

        void updateWorkingDirectory()
        {
            var workingDirectory = engine.GetWorkingDirectory();
            var title = Prompt.GetShellTitleString(
                workingDirectory, engine.RemoteComputerName);

            // Only write shell title if it has changed.
            if (title == prompt.ShellTitle) return;

            // Update prompt object with new shell title.
            prompt.ShellTitle = title;

            // Update the window title to the shell title text.
            Window.GetWindow(this).Title = title;

            // Separate title from other output with a newline beforehand.
            if (Document.TextLength > 0) AppendLine();
            AppendLine(title, VerboseBrush);

            // Update working directory.
            Directory.SetCurrentDirectory(workingDirectory);
        }

        public void WriteOutputLine(string text)
        {
            WriteCustom(text, OutputBrush, true);
        }

        public void WriteErrorLine(string text)
        {
            WriteCustom(text, ErrorBrush, true);
        }

        public void WriteCustom(string text, Brush foreground, bool newline)
        {
            Dispatcher.InvokeAsync(
                () => write(text, foreground, newline), TextPriority);
        }

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

            if (newline) AppendLine(text, foreground);
            else Append(text, foreground);
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
                case Key.Escape:
                    e.Handled = handleEscKey();
                    break;

                case Key.Tab:
                    e.Handled = handleTabKey();
                    break;

                case Key.Left:
                    e.Handled = handleLeftKey();
                    break;

                case Key.Up:
                case Key.Down:
                    e.Handled = handleUpDownKeys(e.Key);
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt.
            if (!readOnlyProvider.IsReadOnly && TextArea.Selection.IsEmpty)
            {
                setInput(string.Empty);
                return true;
            }
            return false;
        }

        bool handleTabKey()
        {
            // Perform autocomplete if at the shell prompt.
            if (!readOnlyProvider.IsReadOnly && prompt.FromShell)
            {
                performAutocomplete(!WPFUtils.IsShiftKeyDown).ContinueWith(
                    t => TextArea.Caret.BringCaretToView(),
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            return true;
        }

        bool handleLeftKey() => CaretOffset == readOnlyEndOffset;

        bool handleUpDownKeys(Key key)
        {
            // Perform history lookup if the caret is after the prompt and 
            // input is allowed. Otherwise, navigate standardly.
            if (CaretOffset >= readOnlyEndOffset)
            {
                if (!readOnlyProvider.IsReadOnly)
                {
                    setInputFromHistory(key == Key.Down);
                    TextArea.Caret.BringCaretToView();
                }
                return true;
            }
            return false;
        }

        int inputLength
        {
            get => Document.TextLength - readOnlyProvider.PromptEndOffset;
        }

        string getInput()
        {
            return Document.GetText(
                readOnlyProvider.PromptEndOffset, inputLength);
        }

        void setInput(string text)
        {
            Document.Replace(
                readOnlyProvider.PromptEndOffset, inputLength, text);
        }

        async Task performAutocomplete(bool forward)
        {
            // Get data needed for the completion lookup.
            var input = getInput();
            var index = CaretOffset - readOnlyProvider.PromptEndOffset;

            // Retrieve the completion text.
            var replacementIndex = default(int);
            var replacementLength = default(int);
            var completionText = default(string);
            var hasCompletion = await Task.Run(
                () => engine.MyHost.CurrentPipeline.Autocomplete.GetCompletion(
                    input, index, forward,
                    out replacementIndex,
                    out replacementLength,
                    out completionText));
            if (!hasCompletion) return;

            // Update the input (UI) with the result of the completion.
            replacementLength = string.IsNullOrWhiteSpace(input)
                ? input.Length : replacementLength;
            Document.Replace(
                replacementIndex + readOnlyProvider.PromptEndOffset,
                replacementLength,
                completionText);
        }

        void setInputFromHistory(bool forward)
        {
            // Disallow lookup when not at the shell prompt.
            if (!prompt.FromShell) return;

            // Cache the current user input to the buffer and look up the stored 
            // input to display from the buffer.
            var storedInput = engine.MyHistory.Cycle(getInput(), forward);

            // Return if no command was found.
            if (storedInput == null) return;

            // Update the display to show the new input.
            setInput(storedInput);
            CaretOffset = Document.TextLength;
        }

        void execute()
        {
            if (readOnlyProvider.IsReadOnly) return;

            disableChangingText();
            var input = getInput();
            AppendLine();

            // If this is the shell prompt, execute the input.
            if (prompt.FromShell) Task.Run(() => executeInput(input));
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
        {
            Dispatcher.InvokeAsync(() => Window.GetWindow(this).Close());
        }
    }
}
