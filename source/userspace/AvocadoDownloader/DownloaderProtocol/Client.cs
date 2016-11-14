using StandardLibrary.Processes.NamedPipes;
using System;
using System.Threading.Tasks;

namespace DownloaderProtocol
{
    public sealed class Client : IDisposable
    {
        readonly NamedPipeClient pipeClient;

        public static async Task<Client> Create()
        {
            var pipeClient = new NamedPipeClient(ProtocolConfig.PipeName);
            await pipeClient.Connect();
            return new Client(pipeClient);
        }

        Client(NamedPipeClient pipeClient)
        {
            this.pipeClient = pipeClient;
        }

        public Task SendMessage(
            MessageType messageType, string title, string filePath, string data)
        {
            var type = ((int)messageType).ToString();
            return sendMessage(type, title, filePath, data);
        }

        Task sendMessage(params string[] args)
        {
            var message = string.Join(ProtocolConfig.MessageDelimiter, args);
            return pipeClient.Send(ProtocolConfig.MessageDelimiter + message);
        }

        public void Dispose() => pipeClient.Dispose();
    }
}