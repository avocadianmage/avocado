using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvocadoDownloader.BusinessLayer
{
    public class Grouper
    {
        public string Title { get; }
        public ObservableCollection<FileItem> FileItems { get; }
            = new ObservableCollection<FileItem>();
        readonly Dictionary<string, FileItem> fileItemLookup
             = new Dictionary<string, FileItem>();

        public Grouper(string title, IEnumerable<string> filePaths)
        {
            Title = title;
            foreach (var filePath in filePaths)
            {
                var fileItem = new FileItem(filePath);
                FileItems.Add(fileItem);
                fileItemLookup.Add(filePath, fileItem);
            }
        }

        public FileItem this[string key] => fileItemLookup[key];
    }
}