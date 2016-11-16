using StandardLibrary.Utilities;
using System.Collections.Generic;

namespace AvocadoDownloader.BusinessLayer
{
    public class Grouper
    {
        public string Title { get; }
        public ObservableDictionary<string, FileItem> FileItems { get; }
            = new ObservableDictionary<string, FileItem>();

        public Grouper(string title, IEnumerable<string> filePaths)
        {
            Title = title;
            AddFileItems(filePaths);
        }

        public void AddFileItems(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (FileItems.ContainsKey(filePath)) continue;
                FileItems.Add(filePath, new FileItem(filePath));
            }
        }
    }
}