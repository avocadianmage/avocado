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

            //ckgtest
            dataModel.AddGrouper("Angel Beats!",
                new List<string> { "Episode-001" });
            dataModel.AddGrouper("Puella Magi Madoka Magica",
                new List<string> { "Episode-004", "Episode-005" });
            dataModel.AddGrouper("One Punch Man",
                new List<string> { "Episode-002" });
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
