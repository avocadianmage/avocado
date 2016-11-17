using StandardLibrary.Utilities;
using System.Net;
using System.Threading.Tasks;

namespace StandardLibrary.Web
{
    public sealed class WebDownload
    {
        public event DownloadProgressChangedEventHandler ProgressUpdated;

        readonly WebClient downloadWebClient = new WebClient();
        readonly string savePath;

        public WebDownload(string savePath)
        {
            this.savePath = savePath;
        }

        // Downloads the content at the specified URL and saves it to the 
        // specified filepath.
        public Task Start(string url)
        {
            downloadWebClient.DownloadProgressChanged += ProgressUpdated;
            return downloadWebClient.DownloadFileTaskAsync(url, savePath);
        }

        public async Task Cancel()
        {
            downloadWebClient.DownloadProgressChanged -= ProgressUpdated;
            downloadWebClient.CancelAsync();

            // Wait until the download operation releases the lock on the file.
            while (IOUtils.IsFileLocked(savePath)) await Task.Delay(10);
        }
    }
}