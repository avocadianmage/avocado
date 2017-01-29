using AvocadoFramework.Controls.TextRendering;
using StandardLibrary.WPF;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace AvocadoShell.UI.Editor
{
    sealed class EditorTextArea : InputTextArea
    {
        string path;

        public void OpenFile(string path)
        {
            this.path = path;

            ClearAllText();
            Write(File.ReadAllText(path), Foreground);
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
                new TextRange(StartPointer, EndPointer)
                    .Save(stream, DataFormats.Text);
            }
        }
    }
}