using AvocadoFramework.Engine;
using StandardLibrary.Extensions;
using StandardLibrary.Web.Scraping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;

namespace AvocadoDownloader
{
    public partial class MainWindow : GlassPane
    {
        readonly DataModel dataModel = new DataModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = dataModel;
        }
    }

    public class DataModel
    {
        public ObservableCollection<Grouper> Groupers { get; }
            = new ObservableCollection<Grouper>();

        public void AddGrouper(string title, IEnumerable<string> filePaths)
            => Groupers.Add(new Grouper(title, filePaths));
    }

    public class Grouper
    {
        public string Title { get; }
        public ObservableCollection<FileItem> FileItems { get; }
            = new ObservableCollection<FileItem>();

        public Grouper(string title, IEnumerable<string> filePaths)
        {
            Title = title;
            filePaths.ForEach(fp => FileItems.Add(new FileItem(fp)));
        }
    }

    public class FileItem
    {
        public string FilePath { get; }
        public string Status { get; private set; }
        public double Value { get; private set; }

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
            Value = percent;

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
    }
}
