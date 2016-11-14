using DownloaderProtocol;
using StandardLibrary.Processes;
using StandardLibrary.Processes.NamedPipes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvocadoDownloader
{
    static class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Title is the first argument. Quit if it was not sent.
            var title = args.FirstOrDefault();
            if (title == null) return;

            // Subsequent arguments are file paths grouped under the title.
            // Quit if this set is empty.
            var filePaths = args.Skip(1);
            if (!filePaths.Any()) return;

            // Check if the downloader is already running.
            var mutex = new Mutex(false, Config.MutexName);
            try
            {
                if (mutex.WaitOne(0, false)) App.Main();
                else sendDataToFirstInstance().Wait();
            }
            finally { mutex?.Close(); }
        }

        static async Task sendDataToFirstInstance()
        {
            using (var client = new NamedPipeClient(ProtocolConfig.PipeName))
            {
                await client.Connect();
                await client.Send(EnvUtils.BuildArgStr());
            }
        }
    }
}