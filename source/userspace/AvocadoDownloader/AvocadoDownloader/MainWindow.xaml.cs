using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;

namespace AvocadoDownloader
{
    public partial class MainWindow : GlassPane
    {
        readonly DataModel dataModel = new DataModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = dataModel;
        }
    }
}
