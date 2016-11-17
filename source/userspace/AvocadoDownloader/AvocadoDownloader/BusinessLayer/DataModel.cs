using StandardLibrary.Utilities;
using System;
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
            if (Groupers.ContainsKey(title))
            {
                Groupers[title].AddFileItems(filePaths);
                return;
            }

            var grouper = new Grouper(title, filePaths);
            grouper.Removed += (s, e) => Groupers.Remove(((Grouper)s).Title);
            Groupers.Add(title, grouper);
        }
    }
}