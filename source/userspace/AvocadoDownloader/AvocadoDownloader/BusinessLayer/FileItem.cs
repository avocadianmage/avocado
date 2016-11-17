using StandardLibrary.Extensions;
using StandardLibrary.Web.Scraping;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AvocadoDownloader.BusinessLayer
{
    public class FileItem : INotifyPropertyChanged
    {
        public event EventHandler Removed;

        string status;
        double progressValue;

        public string FilePath { get; }
        public string Status
        {
            get { return status; }
            set
            {
                if (status == value) return;
                status = value;
                PropertyChanged?.Invoke(
                    this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }
        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (progressValue == value) return;
                progressValue = value;
                PropertyChanged?.Invoke(
                    this, new PropertyChangedEventArgs(nameof(ProgressValue)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        readonly StaticScraper scraper = new StaticScraper();

        public FileItem(string filePath)
        {
            FilePath = filePath;
            Status = Config.InitialDownloadStatus;
            scraper.ProgressUpdated += onProgressUpdated;
        }

        public async Task DownloadFromUrl(string url)
        {
            NotifyStart();
            scraper.CancelDownload();
            await scraper.Download(url, FilePath);
            NotifyFinish();
        }

        public void NotifyStart() => Status = Config.StartDownloadStatus;
        public void NotifyFinish() => Status = Config.FinishDownloadStatus;
        public void NotifyProgress(double percent)
             => updateProgress(percent, null);

        void onProgressUpdated(
            object sender, DownloadProgressChangedEventArgs e)
        {
            updateProgress(e.BytesReceived, e.TotalBytesToReceive);
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
            ProgressValue = percent;

            // Update status text.
            var status = $"{percent.ToRoundedString(2)}%";
            if (!string.IsNullOrWhiteSpace(downloadBytesStr))
                status += $" [{downloadBytesStr}]";
            Status = status;
        }

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

        public void Run()
        {
            if (File.Exists(FilePath)) Process.Start(FilePath);
        }

        public void Remove()
        {
            Removed(this, EventArgs.Empty);
        }
    }
}