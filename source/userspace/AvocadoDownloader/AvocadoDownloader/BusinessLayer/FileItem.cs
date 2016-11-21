using StandardLibrary.Utilities.Extensions;
using StandardLibrary.Web;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvocadoDownloader.BusinessLayer
{
    public class FileItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Removed;

        public string FilePath { get; }
        public bool FinishedDownloading { get; private set; }

        string status;
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

        double progressValue;
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

        readonly WebDownload webDownload;

        public FileItem(string filePath)
        {
            webDownload = new WebDownload(filePath);
            webDownload.ProgressUpdated += onProgressUpdated;

            FilePath = filePath;
            Status = Config.InitialDownloadStatus;
        }

        public async Task DownloadFromUrl(string url)
        {
            NotifyStart();
            await webDownload.Start(url);
            NotifyFinish();
        }

        public async Task PrepareForDownload(string status)
        {
            await webDownload.Cancel();
            Status = status;
        }

        public void NotifyStart()
        {
            Status = Config.StartDownloadStatus;
            ProgressValue = 0;
            FinishedDownloading = false;
        }

        public void NotifyFinish()
        {
            Status = Config.FinishDownloadStatus;
            ProgressValue = 100;
            FinishedDownloading = true;
        }

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

        public void Remove(bool deleteFromDisk)
        {
            Task.Run(async () =>
            {
                // Ensure the download is stopped.
                await webDownload.Cancel();

                // If requested, delete the file from disk.
                if (!deleteFromDisk) return;
                File.Delete(FilePath);
                deleteDirectoryIfEmpty(Path.GetDirectoryName(FilePath));
            });

            // Notify listeners (ex: parent grouper) that the item should be 
            // removed.
            Removed(this, EventArgs.Empty);
        }

        void deleteDirectoryIfEmpty(string path)
        {
            if (Directory.EnumerateFileSystemEntries(path).Any()) return;
            Directory.Delete(path);
        }
    }
}