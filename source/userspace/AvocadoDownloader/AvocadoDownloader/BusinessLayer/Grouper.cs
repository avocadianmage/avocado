using StandardLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoDownloader.BusinessLayer
{
    public class Grouper
    {
        public event EventHandler Removed;

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

                var fileItem = new FileItem(filePath);
                fileItem.Removed += onFileItemRemoved;
                FileItems.Add(filePath, fileItem);
            }
        }

        void onFileItemRemoved(object sender, EventArgs e)
        {
            var target = (FileItem)sender;
            FileItems.Remove(target.FilePath, target);

            // If there are no more items in this grouper, remove it.
            if (!FileItems.Any()) Remove();
        }

        public void Remove()
        {
            Removed(this, EventArgs.Empty);
        }
    }
}