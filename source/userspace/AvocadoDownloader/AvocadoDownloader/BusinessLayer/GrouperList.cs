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

        public Grouper AddGrouper(
            string directoryPath, IEnumerable<string> filePaths)
        {
            if (grouperDict.ContainsKey(directoryPath))
            {
                grouperDict[directoryPath].AddFileItems(filePaths);
                return grouperDict[directoryPath];
            }

            var grouper = new Grouper(directoryPath, filePaths);
            grouper.Removed += (s, e) 
                => grouperDict.Remove(((Grouper)s).DirectoryPath);
            grouperDict.Add(directoryPath, grouper);
            return grouper;
        }
    }
}