using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;
using DownloaderProtocol;
using StandardLibrary.Processes;
using StandardLibrary.Processes.NamedPipes;
using StandardLibrary.Utilities.Extensions;
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
                => processMessage(e.Message);
            await server.Start(ProtocolConfig.PipeName);
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
            Activate();
        }

        void processMessage(string message)
        {
            if (message.StartsWith(ProtocolConfig.MessageDelimiter))
                processProtocolMessage(message);
            // If the message did not start with an expected delimiter, process
            // it as coming from the commandline.
            else
            {
                Dispatcher.BeginInvoke((Action)(() => 
                    processCommandlineArgs(EnvUtils.SplitArgStr(message))));
            }
        }

        void processProtocolMessage(string message)
        {
            var pieces = message.Split(
                ProtocolConfig.MessageDelimiter.Yield().ToArray(),
                StringSplitOptions.RemoveEmptyEntries);
            var messageType = (MessageType)int.Parse(pieces[0]);
            string title = pieces[1], filePath = pieces[2], data = pieces[3];
            var fileItem = dataModel.GetGrouper(title).GetFileItem(filePath);
            switch (messageType)
            {
                case MessageType.PrepareForDownload:
                    fileItem.PrepareForDownload(data).RunAsync();
                    break;

                case MessageType.DownloadFromUrl:
                    fileItem.DownloadFromUrl(data).RunAsync();
                    break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            deleteIncompleteFileItems();
        }

        // Delete file items that have not finished downloading.
        void deleteIncompleteFileItems()
        {
            foreach (var grouper in dataModel.Groupers.ToList())
                foreach (var fileItem in grouper.FileItems.ToList())
                {
                    if (fileItem.FinishedDownloading) continue;
                    fileItem.Remove(true);
                }
        }
    }
}
