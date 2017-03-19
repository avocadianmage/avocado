using AvocadoFramework.Controls.TextRendering;
using AvocadoShell.UI.Utilities;
using StandardLibrary.Utilities.Extensions;
using StandardLibrary.WPF;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace AvocadoShell.UI.Editor
{
    sealed class EditorTextArea : InputTextArea
    {
        readonly PSSyntaxHighlighter highlighter = new PSSyntaxHighlighter();

        string path;

        public EditorTextArea() => TextChanged += onTextChanged;

        async void onTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!path.HasFileExtension(".ps1")) return;
            await highlighter.Highlight(this, GetFullTextRange());
        }

        public void OpenFile(string path)
        {
            this.path = path;

            highlighter.Reset();
            ClearAllText();
            if (File.Exists(path)) Write(File.ReadAllText(path), Foreground);
            CaretPosition = Document.ContentStart;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                case Key.Escape:
                    // If no text was selected, close the editor.
                    if (Selection.IsEmpty) closeEditor();
                    break;

                case Key.S:
                    if (WPFUtils.IsControlKeyDown) saveFile();
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        void closeEditor()
            => ((MainWindow)Window.GetWindow(this)).CloseEditor();

        void saveFile() => File.WriteAllText(path, GetFullTextRange().Text);
    }
}