using StandardLibrary.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace AvocadoShell.UI.Editor
{
    sealed class TerminalCommandBindings
    {
        readonly TerminalEditor editor;
        readonly TerminalReadOnlySectionProvider readOnlyProvider;
        readonly Prompt prompt;

        public TerminalCommandBindings(
            TerminalEditor editor, 
            TerminalReadOnlySectionProvider readOnlyProvider,
            Prompt prompt)
        {
            this.editor = editor;
            this.readOnlyProvider = readOnlyProvider;
            this.prompt = prompt;
        }

        bool isCaretOnPromptLine()
        {
            var promptLine = editor.Document
                .GetLineByOffset(readOnlyProvider.PromptEndOffset).LineNumber;
            return editor.TextArea.Caret.Line == promptLine;
        }

        bool isCaretAfterPrompt
        {
            get
            {
                return !editor.IsReadOnly
                    && editor.CaretOffset >= readOnlyProvider.PromptEndOffset;
            }
        }

        void moveCaretToPromptEnd(bool select)
        {
            var promptEnd = readOnlyProvider.PromptEndOffset;
            if (select) editor.Select(editor.SelectionStart, promptEnd);
            else editor.CaretOffset = promptEnd;
        }

        void modifyCommandBinding(
            ICommand command,
            ICollection<CommandBinding> bindingCollection,
            CanExecuteRoutedEventHandler canExecute)
        {
            bindingCollection
                .Where(b => command == null || b.Command == command)
                .ForEach(b => b.CanExecute += canExecute);
        }

        void bindSelectAll(ICollection<CommandBinding> bindingCollection)
        {
            // Rewire 'Select All' to only select input.
            modifyCommandBinding(
                ApplicationCommands.SelectAll,
                bindingCollection,
                (s, e) => e.CanExecute = overrideSelectAll());
        }

        bool overrideSelectAll()
        {
            if (isCaretAfterPrompt)
            {
                editor.Select(
                    readOnlyProvider.PromptEndOffset, 
                    editor.Document.TextLength);
            }
            return false;
        }

        void bindHome(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                EditingCommands.MoveToDocumentStart,
                bindingCollection,
                (s, e) => e.CanExecute = overrideHome(true, false));
            modifyCommandBinding(
                EditingCommands.SelectToDocumentStart,
                bindingCollection,
                (s, e) => e.CanExecute = overrideHome(true, true));
            modifyCommandBinding(
                EditingCommands.MoveToLineStart,
                bindingCollection,
                (s, e) => e.CanExecute = overrideHome(false, false));
            modifyCommandBinding(
                EditingCommands.SelectToLineStart,
                bindingCollection,
                (s, e) => e.CanExecute = overrideHome(false, true));
        }

        bool overrideHome(bool wholeDocument, bool select)
        {
            if (!isCaretAfterPrompt) return false;
            // Allow moving the caret to the end of the current line to be 
            // handled natively, provided the caret is not on the prompt line.
            if (!wholeDocument && !isCaretOnPromptLine()) return true;
            // Otherwise, move the caret to the end of the prompt.
            moveCaretToPromptEnd(select);
            return false;
        }

        void disableNavigationCommands(
            ICollection<CommandBinding> bindingCollection)
        {
            new[]
            {
                EditingCommands.MoveUpByPage,
                EditingCommands.MoveDownByPage,
                EditingCommands.SelectUpByPage,
                EditingCommands.SelectDownByPage,
                EditingCommands.SelectUpByLine,
                EditingCommands.SelectDownByLine
            }.ForEach(command => modifyCommandBinding(
                command, bindingCollection, (s, e) => e.CanExecute = false));
        }

        void bindLeft(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                EditingCommands.MoveLeftByCharacter,
                bindingCollection,
                (s, e) => e.CanExecute = overrideMoveLeft());
            modifyCommandBinding(
                EditingCommands.SelectLeftByCharacter,
                bindingCollection,
                (s, e) => e.CanExecute = overrideMoveLeft());
            modifyCommandBinding(
               EditingCommands.MoveLeftByWord,
               bindingCollection,
               (s, e) => e.CanExecute = overrideMoveLeftByWord());
            modifyCommandBinding(
               EditingCommands.SelectLeftByWord,
               bindingCollection,
               (s, e) => e.CanExecute = overrideMoveLeftByWord());
        }

        bool overrideMoveLeft()
        {
            return isCaretAfterPrompt
                && editor.CaretOffset != readOnlyProvider.PromptEndOffset;
        }

        bool overrideMoveLeftByWord()
        {
            if (!isCaretAfterPrompt) return false;
            var promptEnd = readOnlyProvider.PromptEndOffset;
            var stringFromPromptEndToCaret = editor.Document.GetText(
                promptEnd, editor.CaretOffset - promptEnd);
            return !string.IsNullOrWhiteSpace(stringFromPromptEndToCaret);
        }

        void bindEnter(ICollection<CommandBinding> bindingCollection)
        {
            // Rewire paragraph break command to execute the prompt input.
            modifyCommandBinding(
                EditingCommands.EnterParagraphBreak,
                bindingCollection,
                (s, e) => e.CanExecute = overrideEnterParagraphBreak());

            // Disable adding newlines during a secure prompt.
            modifyCommandBinding(
                EditingCommands.EnterLineBreak,
                bindingCollection,
                (s, e) => e.CanExecute = !prompt.IsSecure);
        }

        bool overrideEnterParagraphBreak()
        {
            if (isCaretAfterPrompt) editor.ProcessInput();
            return false;
        }

        void bindBackspace(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                EditingCommands.Backspace,
                bindingCollection,
                (s, e) => e.CanExecute = overrideBackspace());
        }

        bool overrideBackspace()
        {
            if (!prompt.IsSecure) return true;
            var secureString = prompt.SecureStringInput;
            var indexToRemove = secureString.Length - 1;
            if (indexToRemove >= 0) secureString.RemoveAt(indexToRemove);
            return false;
        }

        void bindPaste(ICollection<CommandBinding> bindingCollection)
        {
            // Handle pasted text during a secure prompt differently.
            modifyCommandBinding(
                ApplicationCommands.Paste,
                bindingCollection,
                (s, e) => e.CanExecute = overridePaste());
        }

        bool overridePaste()
        {
            if (!prompt.IsSecure) return true;
            Clipboard.GetText().ForEach(prompt.SecureStringInput.AppendChar);
            return false;
        }

        void bindTab(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                EditingCommands.TabForward,
                bindingCollection,
                (s, e) => e.CanExecute = overrideTab(true));
            modifyCommandBinding(
                EditingCommands.TabBackward,
                bindingCollection,
                (s, e) => e.CanExecute = overrideTab(false));
        }

        bool overrideTab(bool forward)
        {
            if (!isCaretAfterPrompt || prompt.IsSecure) return false;
            // Allow native tabbing unless at a shell prompt.
            if (!prompt.FromShell) return true;
            editor.PerformAutocomplete(forward).RunAsync();
            return false;
        }

        void bindUpDown(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                EditingCommands.MoveUpByLine,
                bindingCollection,
                (s, e) => e.CanExecute = overrideUpDown(false));
            modifyCommandBinding(
                EditingCommands.MoveDownByLine,
                bindingCollection,
                (s, e) => e.CanExecute = overrideUpDown(true));
        }

        bool overrideUpDown(bool down)
        {
            if (isCaretAfterPrompt) editor.SetInputFromHistory(down);
            return false;
        }

        void bindCopy(ICollection<CommandBinding> bindingCollection)
        {
            modifyCommandBinding(
                ApplicationCommands.Copy,
                bindingCollection,
                (s, e) => e.CanExecute = overrideCopy());
        }

        bool overrideCopy()
        {
            if (!editor.TextArea.Selection.IsEmpty) return true;
            editor.TerminateExecution();
            return false;
        }

        void bindEscape()
        {
            modifyCommandBinding(
                editor.EscapeCommand,
                editor.TextArea.DefaultInputHandler.CommandBindings,
                (s, e) => e.CanExecute = overrideEscape());
        }

        bool overrideEscape()
        {
            if (!editor.IsReadOnly && editor.TextArea.Selection.IsEmpty)
            {
                editor.SetInput(string.Empty);
                return false;
            }
            return true;
        }

        void bindCaretNavigation()
        {
            var caretNavigationBindings = editor.TextArea.DefaultInputHandler
                .CaretNavigation.CommandBindings;

            // Prevent caret navigation if in readonly-mode or if the caret is 
            // not after the prompt.
            modifyCommandBinding(
                null,
                caretNavigationBindings,
                (s, e) => e.CanExecute = isCaretAfterPrompt);

            disableNavigationCommands(caretNavigationBindings);
            bindSelectAll(caretNavigationBindings);
            bindHome(caretNavigationBindings);
            bindLeft(caretNavigationBindings);
            bindUpDown(caretNavigationBindings);
        }

        void bindEditing()
        {
            var editingCommandBindings = editor.TextArea.DefaultInputHandler
                .Editing.CommandBindings;

            bindEnter(editingCommandBindings);
            bindBackspace(editingCommandBindings);
            bindPaste(editingCommandBindings);
            bindTab(editingCommandBindings);
            bindCopy(editingCommandBindings);
        }

        public void Set()
        {
            bindCaretNavigation();
            bindEditing();
            bindEscape();
        }
    }
}
