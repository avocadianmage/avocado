using AvocadoDownloader.BusinessLayer;
using AvocadoFramework.Engine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using StandardLibrary.Processes.NamedPipes;

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
            Task.Run(initServer);
        }

        async Task initServer()
        {
            var server = new NamedPipeServer();

            await server.Start(Config.PipeName);
        }
    }
}
