using AvocadoFramework.Controls.Progress;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AvocadoDownloader
{
    class FileProgressor : Progressor
    {
        public static readonly DependencyProperty SaveFilePathProperty
            = DependencyProperty.Register(
                "SaveFilePath",
                typeof(string),
                typeof(FileProgressor),
                new FrameworkPropertyMetadata(onSaveFilePathChanged));

        public string SaveFilePath
        {
            get { return GetValue(SaveFilePathProperty) as string; }
            set { SetValue(SaveFilePathProperty, value); }
        }

        static void onSaveFilePathChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Progressor).Title = Path.GetFileName(e.NewValue as string);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    runFile();
                    break;
            }
        }

        void runFile()
        {
            if (File.Exists(SaveFilePath)) Process.Start(SaveFilePath);
        }
    }
}