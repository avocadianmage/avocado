using StandardLibrary.Utilities;
using System;
using System.Collections.Generic;

namespace AvocadoDownloader.BusinessLayer
{
    [Serializable]
    public class DataModel
    {
        public IEnumerable<Grouper> Groupers => grouperDict.EnumerableData;

        readonly ObservableDictionary<string, Grouper> grouperDict
            = new ObservableDictionary<string, Grouper>();

        public Grouper GetGrouper(string title) => grouperDict[title];

        public void AddGrouper(string title, IEnumerable<string> filePaths)
        {
            if (grouperDict.ContainsKey(title))
            {
                grouperDict[title].AddFileItems(filePaths);
                return;
            }

            var grouper = new Grouper(title, filePaths);
            grouper.Removed += (s, e) => grouperDict.Remove(((Grouper)s).Title);
            grouperDict.Add(title, grouper);
        }
    }
}