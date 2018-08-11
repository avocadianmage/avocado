﻿using AvocadoFramework.Controls.TextRendering;
using AvocadoLib.CommandLine.ANSI;
using AvocadoShell.PowerShellService;
using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System.Collections.Generic;
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
            setCommandBindings();
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

        int readOnlyEndOffset
        {
            get
            {
                return IsReadOnly
                    ? Document.TextLength : readOnlyProvider.PromptEndOffset;
            }
        }

        bool isCaretOnPromptLine()
        {
            var promptLine = Document
                .GetLineByOffset(readOnlyProvider.PromptEndOffset).LineNumber;
            return TextArea.Caret.Line == promptLine;
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            var lineBufferLength = getLineBufferLength();
            Task.Run(() => startCommandline(lineBufferLength));

            SizeChanged += onSizeChanged;
        }

        void modifyCommandBinding(
            ICommand command,
            ICollection<CommandBinding> bindingCollection,
            bool removeExisting,
            CanExecuteRoutedEventHandler canExecuteForExisting,
            CanExecuteRoutedEventHandler canExecuteForNew,
            ExecutedRoutedEventHandler executeForNew)
        {
            var existingBindings
                = bindingCollection.Where(b => b.Command == command);

            if (removeExisting)
            {
                existingBindings.ToList().ForEach(
                    b => bindingCollection.Remove(b));
            }
            else if (canExecuteForExisting != null)
            {
                existingBindings.ForEach(
                    b => b.CanExecute += canExecuteForExisting);
            }

            bindingCollection.Add(
                new CommandBinding(command, executeForNew, canExecuteForNew));
        }

        void bindSelectAll(ICollection<CommandBinding> bindingCollection)
        {
            // Rewire 'Select All' to only select input.
            modifyCommandBinding(
                ApplicationCommands.SelectAll,
                bindingCollection,
                true, null,
                onCaretAtPrompt,
                (s, e) => selectInput());
        }

        void bindHome(ICollection<CommandBinding> bindingCollection)
        {
            void bind(ICommand command, ExecutedRoutedEventHandler execute)
            {
                modifyCommandBinding(
                    command,
                    bindingCollection,
                    true, null,
                    onCaretAtPrompt,
                    execute);
            }

            // Rewire 'Home' commands to not take the cursor past the prompt.
            bind(
                EditingCommands.MoveToDocumentStart,
                (s, e) => moveCaret(readOnlyProvider.PromptEndOffset, false));
            bind(
                EditingCommands.SelectToDocumentStart,
                (s, e) => moveCaret(readOnlyProvider.PromptEndOffset, true));
            bind(
                EditingCommands.MoveToLineStart,
                (s, e) => moveCaretToLineStart(false));
            bind(
                EditingCommands.SelectToLineStart,
                (s, e) => moveCaretToLineStart(true));
        }

        void bindPageUpDown(ICollection<CommandBinding> bindingCollection)
        {
            // Disable 'Page Up' and 'Page Down' commands.
            new[]
            {
                EditingCommands.MoveUpByPage,
                EditingCommands.MoveDownByPage,
                EditingCommands.SelectUpByPage,
                EditingCommands.SelectDownByPage
            }.ForEach(command => modifyCommandBinding(
                command, bindingCollection, true, null, null, null));
        }

        void bindLeft(ICollection<CommandBinding> bindingCollection)
        {
            void onCanExecute(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = !IsReadOnly 
                    && CaretOffset > readOnlyProvider.PromptEndOffset;
            }

            modifyCommandBinding(
                EditingCommands.MoveLeftByCharacter,
                bindingCollection,
                false,
                onCanExecute,
                null, null);
        }

        void bindEnter(ICollection<CommandBinding> bindingCollection)
        {
            // Rewire paragraph break command to execute the prompt input.
            modifyCommandBinding(
                EditingCommands.EnterParagraphBreak,
                bindingCollection,
                true, null,
                onCaretAtPrompt,
                (s, e) => execute());

            // Disable adding newlines during a secure prompt.
            modifyCommandBinding(
                EditingCommands.EnterLineBreak,
                bindingCollection,
                false,
                (s, e) => e.CanExecute = !prompt.IsSecure,
                null, null);
        }

        void bindPaste(ICollection<CommandBinding> bindingCollection)
        {
            // Handle pasted text during a secure prompt differently.
            modifyCommandBinding(
                ApplicationCommands.Paste,
                bindingCollection,
                false,
                (s, e) =>
                {
                    if (prompt.IsSecure)
                    {
                        Clipboard.GetText().ForEach(
                            prompt.SecureStringInput.AppendChar);
                        e.CanExecute = false;
                    }
                }, 
                null, null);
        }

        void setCommandBindings()
        {
            var caretNavigationBindings
                = TextArea.DefaultInputHandler.CaretNavigation.CommandBindings;

            // Prevent caret navigation if in readonly-mode or if the caret is 
            // not after the prompt.
            caretNavigationBindings
                .ForEach(b => b.CanExecute += onCaretAtPrompt);

            bindSelectAll(caretNavigationBindings);
            bindHome(caretNavigationBindings);
            bindPageUpDown(caretNavigationBindings);
            bindLeft(caretNavigationBindings);

            var editingCommandBindings
                = TextArea.DefaultInputHandler.Editing.CommandBindings;

            bindEnter(editingCommandBindings);
            bindPaste(editingCommandBindings);

            // Execution break.
            TextArea.DefaultInputHandler.AddBinding(
                new RoutedCommand(),
                ModifierKeys.Control,
                Key.B,
                (s, e) => terminateExec());
        }
        
        void onCaretAtPrompt(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsReadOnly && CaretOffset >= readOnlyEndOffset;
        }

        void moveCaretToLineStart(bool select)
        {
            var targetOffset = isCaretOnPromptLine() 
                ? readOnlyProvider.PromptEndOffset 
                : Document.GetLineByOffset(CaretOffset).Offset;
            moveCaret(targetOffset, select);
        }

        void moveCaret(int targetOffset, bool select)
        {
            if (select) Select(SelectionStart, targetOffset);
            else CaretOffset = targetOffset;
            TextArea.Caret.BringCaretToView();
        }
        
        void selectInput()
        {
            Select(readOnlyProvider.PromptEndOffset, Document.TextLength);
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

                case Key.Up:
                case Key.Down:
                    e.Handled = handleUpDownKeys(e.Key);
                    break;

                case Key.Back:
                    e.Handled = handleBackspaceKey();
                    break;
            }

            base.OnPreviewKeyDown(e);
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

        bool handleEscKey()
        {
            // If no text was selected, delete all text at the prompt and place
            // cursor there.
            if (!IsReadOnly && TextArea.Selection.IsEmpty)
            {
                setInput(string.Empty);
                resetCaretToDocumentEnd();
                return true;
            }
            return false;
        }

        bool handleTabKey()
        {
            if (IsReadOnly || prompt.IsSecure) return true;

            // Perform autocomplete if at the shell prompt.
            if (prompt.FromShell)
            {
                performAutocomplete(!WPFUtils.IsShiftKeyDown).RunAsync();
                return true;
            }
            return false;
        }

        bool handleUpDownKeys(Key key)
        {
            // Perform history lookup if the caret is after the prompt and 
            // input is allowed. Otherwise, navigate standardly.
            if (CaretOffset >= readOnlyEndOffset)
            {
                if (!IsReadOnly)
                {
                    setInputFromHistory(key == Key.Down);
                    TextArea.Caret.BringCaretToView();
                }
                return true;
            }
            return false;
        }

        bool handleBackspaceKey()
        {
            if (!prompt.IsSecure) return false;
            var secureString = prompt.SecureStringInput;
            var indexToRemove = secureString.Length - 1;
            if (indexToRemove >= 0) secureString.RemoveAt(indexToRemove);
            return true;
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
        }

        async Task performAutocomplete(bool forward)
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

                TextArea.Caret.BringCaretToView();
            }
            finally { IsReadOnly = false; }
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
            resetCaretToDocumentEnd();
        }

        void execute()
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
            TextArea.Caret.BringCaretToView();
        }
    }
}
