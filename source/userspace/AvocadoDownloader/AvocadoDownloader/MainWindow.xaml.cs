using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;
using StandardLibrary.Extensions;
using StandardLibrary.Processes;
using StandardLibrary.Processes.NamedPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            initServer().RunAsync();
            processCommandlineArgs(EnvUtils.GetArgs());
        }

        async Task initServer()
        {
            var server = new NamedPipeServer();
            server.MessageReceived += (s, e) 
                => processCommandlineArgs(EnvUtils.SplitArgStr(e.Message));
            await server.Start(Config.PipeName);
        }

        void processCommandlineArgs(IEnumerable<string> args)
        {
            // Title is the first argument. Quit if it was not sent.
            var title = args.FirstOrDefault();
            if (title == null) return;

            // Subsequent arguments are file paths grouped under the title.
            // Quit if this set is empty.
            var filePaths = args.Skip(1);
            if (!filePaths.Any()) return;

            dataModel.AddGrouper(title, filePaths);
        }

        void downloadFromUrl(string title, string filePath, string url)
            => dataModel[title][filePath].DownloadFromUrl(url).RunAsync();
    }
}
