using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.WPF;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoShell.UI.Editor
{
    sealed class EditorTextArea : InputTextArea
    {
        readonly PSSyntaxHighlighter highlighter = new PSSyntaxHighlighter();

        string path;

        public EditorTextArea()
        {
            TextChanged += onTextChanged;
        }

        async void onTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Path.GetExtension(path).ToLower() != ".ps1") return;
            await highlighter.Highlight(this, GetFullTextRange());
        }

        public void OpenFile(string path)
        {
            this.path = path;

            highlighter.Reset();
            ClearAllText();
            Write(File.ReadAllText(path), Foreground);
            CaretPosition = Document.ContentStart;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Escape:
                    closeEditor();
                    break;
                    
                case Key.S:
                    if (WPFUtils.IsControlKeyDown) saveFile();
                    break;
            }
        }

        void closeEditor()
            => ((MainWindow)Window.GetWindow(this)).CloseEditor();

        void saveFile()
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                GetFullTextRange().Save(stream, DataFormats.Text);
            }
        }
    }
}