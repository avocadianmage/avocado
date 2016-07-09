using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace UtilityLib.Processes.NamedPipes
{
    public sealed class NamedPipeClient : IDisposable
    {
        readonly NamedPipeClientStream client;

        public NamedPipeClient(string name)
        {
            client = new NamedPipeClientStream(name);
        }

        public async Task Connect() => await client.ConnectAsync();

        public void Dispose() => client?.Dispose();

        public async Task Send(string message)
        {
            var writer = new StreamWriter(client);
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }
}