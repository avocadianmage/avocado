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
            Terminal.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
            Editor.Focus();
            Editor.OpenFile(path);
        }

        public void CloseEditor()
        {
            Editor.Visibility = Visibility.Collapsed;
            Terminal.Visibility = Visibility.Visible;
            Terminal.Focus();
        }
    }
}
