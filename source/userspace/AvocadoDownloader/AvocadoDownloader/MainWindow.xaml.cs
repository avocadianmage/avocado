using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;
using DownloaderProtocol;
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
            server.MessageReceived += (s, e) => processMessage(e.Message);
            await server.Start(ProtocolConfig.PipeName);
        }

        void processCommandlineArgs(IEnumerable<string> args)
        {
            dataModel.AddGrouper(args.First(), args.Skip(1));
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
            switch (messageType)
            {
                case MessageType.SetStatus:
                    dataModel.GetFileItem(title, filePath).Status = data;
                    break;

                case MessageType.DownloadFromUrl:
                    downloadFromUrl(title, filePath, data);
                    break;
            }
        }

        void downloadFromUrl(string title, string filePath, string url)
            => dataModel.GetFileItem(title, filePath)
                .DownloadFromUrl(url).RunAsync();
    }
}
