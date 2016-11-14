using StandardLibrary.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvocadoDownloader.BusinessLayer
{
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
}