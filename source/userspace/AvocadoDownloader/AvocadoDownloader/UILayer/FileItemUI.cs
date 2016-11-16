using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Controls.Progress;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AvocadoDownloader.UILayer
{
    class FileItemUI : Progressor
    {
        public static readonly DependencyProperty SaveFilePathProperty
            = DependencyProperty.Register(
                "SaveFilePath",
                typeof(string),
                typeof(FileItemUI),
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

            var fileItem = (FileItem)DataContext;
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    fileItem.Run();
                    break;
            }
        }
    }
}