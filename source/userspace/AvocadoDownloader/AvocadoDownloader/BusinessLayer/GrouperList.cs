using StandardLibrary.Utilities;
using System;
using System.Collections.Generic;

namespace AvocadoDownloader.BusinessLayer
{
    [Serializable]
    public class GrouperList
    {
        public IEnumerable<Grouper> Groupers => grouperDict.EnumerableData;

        readonly ObservableDictionary<string, Grouper> grouperDict
            = new ObservableDictionary<string, Grouper>();

        public Grouper GetGrouper(string directoryPath) 
            => grouperDict[directoryPath];

        public void AddGrouper(
            string directoryPath, IEnumerable<FileItem> fileItems)
        {
            if (grouperDict.ContainsKey(directoryPath))
            {
                grouperDict[directoryPath].AddFileItems(fileItems);
                return;
            }

            var grouper = new Grouper(directoryPath, fileItems);
            grouper.Removed += (s, e) 
                => grouperDict.Remove(((Grouper)s).DirectoryPath);
            grouperDict.Add(directoryPath, grouper);
        }
    }
}