using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Web.Scraping
{
    public class DynamicScaper : IScraper
    {
        static readonly int MsBeforeShowBrowser
            = new TimeSpan(0, 0, 10).Milliseconds;
        static readonly int MsBeforeTimeout
            = new TimeSpan(0, 3, 0).Milliseconds;

        public virtual Task<string> GetSource(string url) 
            => GetSource(url, false);

        public Task<string> GetSource(string url, bool showBrowser)
        {
            var tcs = new TaskCompletionSource<string>();

            // Start the browsing operation on a STA thread.
            var thread = new Thread(
                () => tcs.SetResult(navigate(url, showBrowser)));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return tcs.Task;
        }

        string navigate(string url, bool showBrowser)
        {
            string source = null;

            var browser = new WebBrowser
            {
                ScriptErrorsSuppressed = true,
                Dock = DockStyle.Fill
            };
            var frm = new Form
            {
                Width = 800,
                Height = 600,

                // Prevent background browser from stealing focus.
                Enabled = showBrowser
            };
            frm.Controls.Add(browser);

            // Retrieve source before application exit.
            frm.FormClosing += (s, e) => source = getSourceFromBrowser(browser);
            frm.FormClosed += (s, e) => Application.Exit();

            // Handle showing the browser.
            if (showBrowser)
            {
                frm.Show();

                // If timeout is reached without the user taking action, close
                // the browser.
                Task.Run(async () => await Task.Delay(MsBeforeTimeout))
                    .ContinueWith(t =>
                    {
                        // Grab the source and exit the application.
                        source = getSourceFromBrowser(browser);
                        Application.Exit();
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                // Retrieve the source once it has loaded.
                browser.DocumentCompleted += (s, e) =>
                {
                    // Wait for main document to load.
                    if (browser.Url.BaseUrl() != e.Url.BaseUrl()) return;

                    // Grab the source and exit the application.
                    source = getSourceFromBrowser(browser);
                    Application.Exit();
                };

                // Show browser if no result is returned in a timely manner.
                Task.Run(async () => await Task.Delay(MsBeforeShowBrowser))
                    .ContinueWith(t =>
                    {
                        frm.Enabled = true;
                        frm.Show();
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            
            // Navigate to the URL and block until the application exits.
            browser.Navigate(url);
            Application.Run();

            return source;
        }

        string getSourceFromBrowser(WebBrowser browser)
        {
            dynamic dom = browser.Document?.DomDocument;
            return dom?.documentElement?.innerHTML;
        }
    }
}