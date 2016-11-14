using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvocadoDownloader.BusinessLayer
{
    public class DataModel
    {
        public ObservableCollection<Grouper> Groupers { get; }
            = new ObservableCollection<Grouper>();

        public void AddGrouper(string title, IEnumerable<string> filePaths)
            => Groupers.Add(new Grouper(title, filePaths));
    }
}