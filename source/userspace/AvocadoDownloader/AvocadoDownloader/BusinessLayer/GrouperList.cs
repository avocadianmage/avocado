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

        public Grouper GetGrouper(string title) => grouperDict[title];

        public void AddGrouper(string title, IEnumerable<FileItem> fileItems)
        {
            if (grouperDict.ContainsKey(title))
            {
                grouperDict[title].AddFileItems(fileItems);
                return;
            }

            var grouper = new Grouper(title, fileItems);
            grouper.Removed += (s, e) => grouperDict.Remove(((Grouper)s).Title);
            grouperDict.Add(title, grouper);
        }
    }
}