using StandardLibrary.Processes.NamedPipes;
using System.Threading.Tasks;

namespace DownloaderProtocol
{
    public sealed class DownloaderClient : NamedPipeClient
    {
        public DownloaderClient() : base(ProtocolConfig.PipeName) { }

        public Task SendMessage(
            MessageType messageType, 
            string directoryPath, 
            string filePath, 
            string data)
        {
            var type = ((int)messageType).ToString();
            return sendMessage(type, directoryPath, filePath, data);
        }

        Task sendMessage(params string[] args)
        {
            var message = string.Join(ProtocolConfig.MessageDelimiter, args);
            return Send(ProtocolConfig.MessageDelimiter + message);
        }
    }
}