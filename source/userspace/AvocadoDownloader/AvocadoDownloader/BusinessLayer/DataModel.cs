using StandardLibrary.Utilities;
using System.Collections.Generic;

namespace AvocadoDownloader.BusinessLayer
{
    public class DataModel
    {
        public ObservableDictionary<string, Grouper> Groupers { get; }
            = new ObservableDictionary<string, Grouper>();

        public FileItem GetFileItem(string title, string filePath)
            => Groupers[title].FileItems[filePath];

        public void AddGrouper(string title, IEnumerable<string> filePaths)
        {
            var grouper = new Grouper(title, filePaths);
            Groupers.Add(title, grouper);
        }
    }
}