using AvocadoFramework.Controls.TextRendering;
using AvocadoLib.CommandLine.ANSI;
using AvocadoShell.PowerShellService;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static AvocadoShell.Config;

namespace AvocadoShell.UI.Editor
{
    sealed class TerminalEditor : AvocadoTextEditor, IShellUI
    {
        readonly TerminalReadOnlySectionProvider readOnlyProvider
            = new TerminalReadOnlySectionProvider();
        readonly PowerShellEngine engine = new PowerShellEngine();
        readonly Prompt prompt = new Prompt();
        readonly OutputBuffer outputBuffer = new OutputBuffer();
        readonly ResetEventWithData<string> nonShellPromptDone
            = new ResetEventWithData<string>();

        public TerminalEditor()
        {
            // Set up readonly handling for prompts.
            TextArea.ReadOnlySectionProvider = readOnlyProvider;

            // Disable hyperlinks.
            TextArea.Options.EnableEmailHyperlinks = false;
            TextArea.Options.EnableHyperlinks = false;

            // Subscribe to events.
            Loaded += onLoaded;

            disableChangingText();
            loadSyntaxHighlighting();
            new TerminalCommandBindings(this, readOnlyProvider, prompt).Set();
        }

        public new bool IsReadOnly
        {
            get => readOnlyProvider.IsReadOnly;
            set => readOnlyProvider.IsReadOnly = value;
        }

        void enableChangingText()
        {
            IsReadOnly = false;
            Document.UndoStack.SizeLimit = int.MaxValue;
        }

        void disableChangingText()
        {
            IsReadOnly = true;
            Document.UndoStack.SizeLimit = 0;
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            var lineBufferLength = getLineBufferLength();
            Task.Run(() => startCommandline(lineBufferLength));

            SizeChanged += onSizeChanged;
            Window.GetWindow(this).Closing += (s, e2) => TerminateExecution();
        }

        public void TerminateExecution()
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
            writePrompt(text, true);
            prompt.SecureStringInput.MakeReadOnly();
            return prompt.SecureStringInput;
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
            prompt.IsSecure = secure;
            prompt.SecureStringInput = new SecureString();

            if (fromShell) updateWorkingDirectory();

            // Write prompt text.
            Append(text, PromptBrush);

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

            // Update working directory of this program, if the directory exists
            // (i.e. we're not in the registry)
            if (Directory.Exists(workingDirectory))
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }
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
                ANSICode.GetColorSegments(text)
                    .ForEach(seg => writeANSISegment(seg));
            }
            else Append(text, foreground);

            if (newline) AppendLine();
        }

        void writeANSISegment(ANSISegment segment)
        {
            var brush = segment.Color.HasValue
                ? new SolidColorBrush(segment.Color.Value) : OutputBrush;
            Append(segment.Text, brush);
        }
        
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.Handled) return;

            if (!IsReadOnly && prompt.IsSecure)
            {
                e.Text.ForEach(prompt.SecureStringInput.AppendChar);
                e.Handled = true;
            }

            base.OnPreviewTextInput(e);
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

        public void SetInput(string text)
        {
            if (prompt.IsSecure)
            {
                prompt.SecureStringInput.Clear();
                text.ForEach(prompt.SecureStringInput.AppendChar);
            }
            else
            {
                Document.Replace(
                    readOnlyProvider.PromptEndOffset, inputLength, text);
            }
            resetCaretToDocumentEnd();
        }

        public async Task PerformAutocomplete(bool forward)
        {
            IsReadOnly = true; try
            {
                // Get data needed for the completion lookup.
                var input = getInput();
                var index = CaretOffset - readOnlyProvider.PromptEndOffset;

                // Retrieve the completion text.
                var result = await Task.Run(
                    () => engine.MyHost.CurrentPipeline.Autocomplete
                        .GetCompletion(input, index, forward));
                if (!result.HasValue) return;

                // Update the input with the result of the completion.
                var tuple = result.Value;
                tuple.replacementLength = string.IsNullOrWhiteSpace(input)
                    ? input.Length : tuple.replacementLength;
                Document.Replace(
                    tuple.replacementIndex + readOnlyProvider.PromptEndOffset,
                    tuple.replacementLength,
                    tuple.completionText);

                resetCaretToDocumentEnd();
            }
            finally { IsReadOnly = false; }
        }

        public void SetInputFromHistory(bool forward)
        {
            // Disallow lookup when not at the shell prompt.
            if (!prompt.FromShell) return;

            // Cache the current user input to the buffer and look up the stored 
            // input to display from the buffer.
            var storedInput = engine.MyHistory.Cycle(getInput(), forward);

            // Return if no command was found.
            if (storedInput == null) return;

            // Update the display to show the new input.
            SetInput(storedInput);
        }

        public void ProcessInput()
        {
            disableChangingText();
            var input = getInput();
            
            // Prepare caret for output from the execution.
            resetCaretToDocumentEnd();
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

        void resetCaretToDocumentEnd()
        {
            TextArea.ClearSelection();
            CaretOffset = Document.TextLength;
            ScrollToLine(TextArea.Caret.Line);
        }
    }
}
