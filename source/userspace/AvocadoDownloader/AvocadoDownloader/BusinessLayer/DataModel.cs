using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvocadoDownloader.BusinessLayer
{
    public class DataModel
    {
        public ObservableCollection<Grouper> Groupers { get; }
            = new ObservableCollection<Grouper>();
        readonly Dictionary<string, Grouper> grouperLookup
            = new Dictionary<string, Grouper>();

        public void AddGrouper(string title, IEnumerable<string> filePaths)
        {
            var grouper = new Grouper(title, filePaths);
            Groupers.Add(grouper);
            grouperLookup.Add(title, grouper);
        }

        public Grouper this[string key] => grouperLookup[key];
    }
}