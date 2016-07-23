﻿using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Web.Scraping
{
    public class DynamicScaper : IScraper
    {
        public virtual Task<string> GetSource(string url) 
            => Task.Run(() => GetSource(url, false));

        public string GetSource(string url, bool showBrowser)
        {
            string source = null;

            // Start the browsing operation on a STA thread.
            var thread = new Thread(() => source = navigate(url, showBrowser));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // Wait until the operation is complete.
            thread.Join();

            // Return the result.
            return source;
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

            if (showBrowser) frm.Show();
            else
            {
                browser.DocumentCompleted += (s, e) =>
                {
                    // Wait for main document to load.
                    if (browser.Url.BaseUrl() != e.Url.BaseUrl()) return;
                    
                    // Grab the source and exit the application.
                    source = getSourceFromBrowser(browser);
                    Application.Exit();
                };

                // Show browser if no result is returned in 10 seconds.
                var timer = new System.Windows.Forms.Timer { Interval = 10000 };
                timer.Tick += (sender, e) =>
                {
                    timer.Stop();
                    frm.Enabled = true;
                    frm.Show();
                };
                timer.Start();
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