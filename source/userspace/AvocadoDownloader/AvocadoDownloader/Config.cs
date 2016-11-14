namespace AvocadoDownloader
{
    static class Config
    {
        public static string MutexName => "AvocadoDownloaderMutex";
        public static string PipeName => "AvocadoDownloaderPipe";

        public static string InitialDownloadStatus => "Waiting...";
        public static string StartDownloadStatus => "Starting download...";
        public static string FinishDownloadStatus => "Done";
    }
}