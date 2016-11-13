using AvocadoFramework.Controls.Progress;
using StandardLibrary.Extensions;
using StandardLibrary.Web.Scraping;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AvocadoDownloader
{
    class FileProgressor : Progressor
    {
        public event EventHandler<EventArgs> ProgressUpdated;

        readonly StaticScraper scraper = new StaticScraper();

        public FileProgressor()
        {
            scraper.ProgressUpdated += onProgressUpdated;
            Status = "Waiting...";
        }

        public static readonly DependencyProperty SaveFilePathProperty
            = DependencyProperty.Register(
                "SaveFilePath",
                typeof(string),
                typeof(FileProgressor),
                new FrameworkPropertyMetadata(onSaveFilePathChanged));

        public string SaveFilePath
        {
            get { return GetValue(SaveFilePathProperty) as string; }
            set { SetValue(SaveFilePathProperty, value); }
        }

        static void onSaveFilePathChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Progressor).Title = Path.GetFileName(e.NewValue as string);
        }

        public async Task DownloadFromUrl(string url)
        {
            NotifyStart();

            scraper.CancelDownload();
            await scraper.Download(url, SaveFilePath);

            NotifyComplete();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    runFile();
                    break;
            }
        }

        void runFile()
        {
            if (File.Exists(SaveFilePath)) Process.Start(SaveFilePath);
        }

        void onProgressUpdated(
            object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(
                () => updateProgress(e.BytesReceived, e.TotalBytesToReceive)));
        }

        void updateProgress(long bytes, long totalBytes)
        {
            updateProgress(
                100D * bytes / totalBytes,
                getDownloadBytesStr(bytes, totalBytes));
        }

        void updateProgress(double percent, string downloadBytesStr)
        {
            // Set progress bar value.
            Value = percent;

            // Update status text.
            var status = $"{percent.ToRoundedString(2)}%";
            if (!string.IsNullOrWhiteSpace(downloadBytesStr))
                status += $" [{downloadBytesStr}]";
            Status = status;

            // Notify listeners that the progress was updated.
            ProgressUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyStart() => Status = "Starting download...";
        public void NotifyComplete() => Status = "Ready";
        public void NotifyProgress(double percent)
             => updateProgress(percent, null);

        static string getDownloadBytesStr(long bytes, long totalBytes)
        {
            const int DECIMAL_PLACES = 2;
            const int BYTE_BASE = 1024;
            string[] metricPrefixes = { string.Empty, "k", "M", "G", "T", "P" };

            var index = (int)Math.Log(totalBytes, BYTE_BASE);

            Func<long, string> toDisp = (byteCount) =>
            {
                var num = byteCount / Math.Pow(BYTE_BASE, index);
                return num.ToRoundedString(DECIMAL_PLACES);
            };
            var numDisp = toDisp(bytes);
            var totalNumDisp = toDisp(totalBytes);

            var prefix = metricPrefixes[index];
            return $"{numDisp}/{totalNumDisp} {prefix}B";
        }
    }
}