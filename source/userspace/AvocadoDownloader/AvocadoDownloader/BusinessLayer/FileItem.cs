using StandardLibrary.Utilities.Extensions;
using StandardLibrary.Web.Scraping;
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
                    this, 
                    new PropertyChangedEventArgs(nameof(Status)));
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
                    this, 
                    new PropertyChangedEventArgs(nameof(ProgressValue)));
            }
        }

        bool isIndeterminate;
        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                if (isIndeterminate == value) return;
                isIndeterminate = value;
                PropertyChanged?.Invoke(
                    this, 
                    new PropertyChangedEventArgs(nameof(IsIndeterminate)));
            }
        }

        readonly WebDownload webDownload;

        public FileItem(string filePath)
        {
            webDownload = new WebDownload(filePath);
            webDownload.ProgressUpdated += onProgressUpdated;

            FilePath = filePath;
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
            FinishedDownloading = false;
            IsIndeterminate = true;
        }

        void notifyStart() => Status = Config.StartDownloadStatus;

        void notifyFinish()
        {
            Status = string.Empty;
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

            ProgressValue = percent;
            Status = $@"{percent.ToRoundedString(2)}% [{
                getDownloadBytesStr(bytes, totalBytes)}]";
            IsIndeterminate = false;
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
            // Also delete from disk if the file has not finished downloading.
            deleteFromDisk = deleteFromDisk || !FinishedDownloading;

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

        public void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(FilePath), FilePath);
            info.AddValue(nameof(FinishedDownloading), FinishedDownloading);
            info.AddValue(nameof(Status), Status);
            info.AddValue(nameof(ProgressValue), ProgressValue);
            info.AddValue(nameof(IsIndeterminate), IsIndeterminate);
            info.AddValue(nameof(Removed), Removed);
        }

        FileItem(SerializationInfo info, StreamingContext context)
            : this(info.GetString(nameof(FilePath)))
        {
            FinishedDownloading = info.GetBoolean(nameof(FinishedDownloading));
            Status = info.GetString(nameof(Status));
            ProgressValue = info.GetDouble(nameof(ProgressValue));
            IsIndeterminate = info.GetBoolean(nameof(IsIndeterminate));
            Removed += (EventHandler)info.GetValue(
                nameof(Removed), typeof(EventHandler));
        }
    }
}