using StandardLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoDownloader.BusinessLayer
{
    [Serializable]
    public class Grouper
    {
        public event EventHandler Removed;

        public string Title { get; }
        public IEnumerable<FileItem> FileItems => fileItemDict.EnumerableData;

        readonly ObservableDictionary<string, FileItem> fileItemDict
            = new ObservableDictionary<string, FileItem>();

        public Grouper(string title, IEnumerable<FileItem> fileItems)
        {
            Title = title;
            AddFileItems(fileItems);
        }

        public FileItem GetFileItem(string filePath) => fileItemDict[filePath];

        public void AddFileItems(IEnumerable<FileItem> fileItems)
        {
            foreach (var fileItem in fileItems)
            {
                var filePath = fileItem.FilePath;
                if (fileItemDict.ContainsKey(filePath)) continue;

                fileItem.Removed += onFileItemRemoved;
                fileItemDict.Add(filePath, fileItem);
            }
        }

        void onFileItemRemoved(object sender, EventArgs e)
        {
            var target = (FileItem)sender;
            fileItemDict.Remove(target.FilePath);

            // If there are no more items in this grouper, remove it.
            if (!FileItems.Any()) Removed(this, EventArgs.Empty);
        }

        public void Remove(bool deleteFromDisk)
            => FileItems.ToList().ForEach(f => f.Remove(deleteFromDisk));
    }
}