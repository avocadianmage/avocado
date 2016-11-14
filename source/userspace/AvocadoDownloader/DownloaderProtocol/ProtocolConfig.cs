namespace DownloaderProtocol
{
    public static class ProtocolConfig
    {
        public static string PipeName => "AvocadoDownloaderPipe";
        public static string MessageDelimiter { get; } = ((char)5).ToString();
    }
}