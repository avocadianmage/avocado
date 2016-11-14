using StandardLibrary.Processes;
using StandardLibrary.Processes.NamedPipes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvocadoDownloader
{
    static class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
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
            using (var client = new NamedPipeClient(Config.PipeName))
            {
                await client.Connect();
                await client.Send(EnvUtils.BuildArgStr());
            }
        }
    }
}