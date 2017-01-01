using AvocadoFramework.Controls.TextRendering;
using System.Windows.Media;

namespace AvocadoDownloader
{
    static class Config
    {
        public static string MutexName => "AvocadoDownloaderMutex";

        public static string StartDownloadStatus => "Starting download...";

        public static Color ActiveGrouperColor => TextPalette.Orange.Color;
        public static Color InactiveGrouperColor => TextPalette.LightGreen.Color;
    }
}