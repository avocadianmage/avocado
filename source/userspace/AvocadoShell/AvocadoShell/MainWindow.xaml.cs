using AvocadoFramework.Engine;
using System.Windows;

namespace AvocadoShell
{
    public partial class MainWindow : GlassPane
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OpenEditor(string path)
        {
            toggleMode(Editor, Terminal);
            Editor.OpenFile(path);
        }

        public void CloseEditor() => toggleMode(Terminal, Editor);

        void toggleMode(FrameworkElement active, FrameworkElement inactive)
        {
            inactive.Visibility = Visibility.Collapsed;
            active.Visibility = Visibility.Visible;
            active.Focus();
        }
    }
}
