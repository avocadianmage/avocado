using StandardLibrary.Web.Browser;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandardLibrary.Web.Scraping
{
    public class DynamicScaper
    {
        public bool ShowBrowser { get; set; }

        static readonly int MsBeforeShowBrowser
            = (int)new TimeSpan(0, 0, 10).TotalMilliseconds;
        static readonly int MsBeforeTimeout
            = (int)new TimeSpan(0, 3, 0).TotalMilliseconds;

        public virtual Task<string> GetSource(string url) 
            => GetSource(url, null);
        
        public virtual Task<string> GetSource(
            string url, Dictionary<string, string> postData)
        {
            var tcs = new TaskCompletionSource<string>();

            // Start the browsing operation on a STA thread.
            var thread = new Thread(
                () => tcs.SetResult(navigate(url, postData)));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return tcs.Task;
        }

        string navigate(string url, Dictionary<string, string> postData)
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
                Enabled = ShowBrowser
            };
            frm.Controls.Add(browser);

            // Retrieve source before application exit.
            frm.FormClosing += (s, e) => source = browser.GetSource();
            frm.FormClosed += (s, e) => Application.Exit();

            // Handle showing the browser.
            if (ShowBrowser)
            {
                frm.Show();

                // If timeout is reached without the user taking action, close
                // the browser.
                Task.Run(() => Task.Delay(MsBeforeTimeout))
                    .ContinueWith(t =>
                    {
                        frm.Close();
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
                    source = browser.GetSource();
                    Application.Exit();
                };

                // Show browser if no result is returned in a timely manner.
                Task.Run(() => Task.Delay(MsBeforeShowBrowser))
                    .ContinueWith(t =>
                    {
                        frm.Enabled = true;
                        frm.Show();
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }

            // Navigate to the URL and block until the application exits.
            if (postData == null) browser.Navigate(url);
            else browser.Post(url, postData);

            Application.Run();
            return source;
        }
    }
}