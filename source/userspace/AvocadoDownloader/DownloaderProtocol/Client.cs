using StandardLibrary.Processes.NamedPipes;
using System.Threading.Tasks;

namespace DownloaderProtocol
{
    public sealed class Client
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

        public void SendMessage(
            MessageType messageType, string title, string filePath, string data)
        {
            sendMessage(((int)messageType).ToString(), title, filePath, data);
        }

        void sendMessage(params string[] args)
        {
            var message = string.Join(ProtocolConfig.MessageDelimiter, args);
            pipeClient.Send(ProtocolConfig.MessageDelimiter + message);
        }
    }
}