using StandardLibrary.Processes;
using StandardLibrary.Utilities;
using StandardLibrary.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

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

        public Grouper(string directoryPath, IEnumerable<string> filePaths)
        {
            DirectoryPath = directoryPath;
            Directory.CreateDirectory(directoryPath);

            AddFileItems(filePaths);
        }

        public void Open()
        {
            new ManagedProcess("Shell", "Get-ChildItem")
            {
                WorkingDirectory = DirectoryPath
            }
            .RunForeground();
        }

        public FileItem GetFileItem(string fileName) => fileItemDict[fileName];

        public IEnumerable<FileItem> AddFileItems(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (fileItemDict.ContainsKey(filePath)) continue;

                var fileItem = new FileItem(filePath);
                fileItem.Removed += onFileItemRemoved;
                fileItemDict.Add(filePath, fileItem);
                yield return fileItem;
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

        [OnDeserialized]
        void onDeserialized(StreamingContext streamingContext)
            => FileItems.ForEach(f => f.Removed += onFileItemRemoved);
    }
}