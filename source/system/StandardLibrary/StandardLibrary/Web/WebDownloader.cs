using StandardLibrary.Utilities;
using System.Net;
using System.Threading.Tasks;

namespace StandardLibrary.Web
{
    public sealed class WebDownloader
    {
        public event DownloadProgressChangedEventHandler ProgressUpdated;

        readonly WebClient downloadWebClient = new WebClient();
        string savePath;

        // Downloads the content at the specified URL and saves it to the 
        // specified filepath.
        public async Task Download(string url, string savePath)
        {
            await CancelDownload();

            this.savePath = savePath;
            downloadWebClient.DownloadProgressChanged += ProgressUpdated;
            await downloadWebClient.DownloadFileTaskAsync(url, savePath);
        }

        public async Task CancelDownload()
        {
            downloadWebClient.CancelAsync();

            // Wait until the download operation releases the lock on the file.
            while (IOUtils.IsFileLocked(savePath)) await Task.Delay(10);
        }
    }
}