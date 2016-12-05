using StandardLibrary.Utilities.Extensions;
using StandardLibrary.Web;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AvocadoDownloader.BusinessLayer
{
    [Serializable]
    public class FileItem : INotifyPropertyChanged, ISerializable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Removed;
        public event EventHandler DownloadFinished;

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
            notifyStart();
            await webDownload.Start(url);
            notifyFinish();
        }

        public async Task PrepareForDownload(string status)
        {
            await webDownload.Cancel();
            Status = status;
            ProgressValue = 0;
        }

        void notifyStart()
        {
            Status = Config.StartDownloadStatus;
            ProgressValue = 0;
            FinishedDownloading = false;
        }

        void notifyFinish()
        {
            Status = Config.FinishDownloadStatus;
            ProgressValue = 100;
            FinishedDownloading = true;
            DownloadFinished?.Invoke(this, EventArgs.Empty);
        }

        void onProgressUpdated(
            object sender, DownloadProgressChangedEventArgs e)
        {
            updateProgress(e.BytesReceived, e.TotalBytesToReceive);
        }

        void updateProgress(long bytes, long totalBytes)
        {
            var percent = 100D * bytes / totalBytes;

            // Set progress bar value.
            ProgressValue = percent;

            // Update status text.
            Status = $@"{percent.ToRoundedString(2)}% [{
                getDownloadBytesStr(bytes, totalBytes)}]";
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
            if (string.IsNullOrWhiteSpace(path) 
                || Directory.EnumerateFileSystemEntries(path).Any()) return;
            Directory.Delete(path);
        }

        public void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Removed", Removed);
            info.AddValue("FilePath", FilePath);
            info.AddValue("FinishedDownloading", FinishedDownloading);
            info.AddValue("Status", Status);
            info.AddValue("ProgressValue", ProgressValue);
        }

        FileItem(SerializationInfo info, StreamingContext context)
            : this(info.GetString("FilePath"))
        {
            Removed += (EventHandler)info.GetValue(
                "Removed", typeof(EventHandler));
            FinishedDownloading = info.GetBoolean("FinishedDownloading");
            Status = info.GetString("Status");
            ProgressValue = info.GetDouble("ProgressValue");
        }
    }
}