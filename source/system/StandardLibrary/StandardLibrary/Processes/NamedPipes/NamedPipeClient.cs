using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace StandardLibrary.Processes.NamedPipes
{
    public class NamedPipeClient : IDisposable
    {
        readonly NamedPipeClientStream client;

        public NamedPipeClient(string name)
        {
            client = new NamedPipeClientStream(name);
        }

        public Task Connect() => client.ConnectAsync();
        
        public void Dispose() => client?.Dispose();

        public async Task Send(string message)
        {
            var writer = new StreamWriter(client);
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }
}