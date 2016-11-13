using AvocadoFramework.Engine;
using StandardLibrary.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvocadoDownloader
{
    public partial class MainWindow : GlassPane
    {
        readonly DataModel dataModel = new DataModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = dataModel;
        }
    }

    public class DataModel
    {
        public ObservableCollection<Grouper> Groupers { get; }
            = new ObservableCollection<Grouper>();

        public void AddGrouper(string title, IEnumerable<string> filePaths)
            => Groupers.Add(new Grouper(title, filePaths));
    }

    public class Grouper
    {
        public string Title { get; }
        public ObservableCollection<FileItem> FileItems { get; }
            = new ObservableCollection<FileItem>();

        public Grouper(string title, IEnumerable<string> filePaths)
        {
            Title = title;
            filePaths.ForEach(fp => FileItems.Add(new FileItem(fp)));
        }
    }

    public class FileItem
    {
        public string FilePath { get; }

        public FileItem(string filePath)
        {
            FilePath = filePath;
        }
    }
}
