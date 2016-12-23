using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvocadoDownloader.BusinessLayer
{
    [Serializable]
    public class Grouper
    {
        public event EventHandler Removed;

        public string DirectoryPath { get; }
        public IEnumerable<FileItem> FileItems => fileItemDict.EnumerableData;

        readonly ObservableDictionary<string, FileItem> fileItemDict
            = new ObservableDictionary<string, FileItem>();

        public Grouper(string directoryPath, IEnumerable<FileItem> fileItems)
        {
            DirectoryPath = directoryPath;
            Directory.CreateDirectory(directoryPath);

            AddFileItems(fileItems);
        }

        public void Open()
        {
            Directory.SetCurrentDirectory(DirectoryPath);
            new ManagedProcess("Shell").RunForeground();
        }

        public FileItem GetFileItem(string fileName) => fileItemDict[fileName];

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