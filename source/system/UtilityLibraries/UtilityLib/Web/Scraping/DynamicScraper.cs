using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Web.Scraping
{
    public class DynamicScaper : IScraper
    {
        string source;
        bool showBrowser;

        public virtual async Task<string> GetSource(string url)
        {
            return await GetSource(url, false);
        }

        public async Task<string> GetSource(string url, bool showBrowser)
        {
            reset(showBrowser);
            return await Task.Run(() => performBrowsing(url));
        }

        void reset(bool showBrowser)
        {
            source = null;
            this.showBrowser = showBrowser;
        }

        string performBrowsing(string url)
        {
            // Start the browsing operation on a STA thread.
            var thread = new Thread(() => navigate(url));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // Wait until the operation is complete.
            thread.Join();

            // Return the result.
            return source;
        }

        void navigate(string url)
        {
            var browser = new WebBrowser
            {
                ScriptErrorsSuppressed = true,
                Dock = DockStyle.Fill,
            };
            browser.DocumentCompleted += documentCompleted;
            browser.Navigate(url);

            if (showBrowser)
            {
                var frm = new Form
                {
                    Width = 600,
                    Height = 600,
                };
                frm.Controls.Add(browser);
                frm.FormClosing += (sender, e) =>
                {
                    source = getSourceFromBrowser(browser);
                };

                Application.Run(frm);
            }
            else
            {
                var timer = new System.Windows.Forms.Timer
                {
                    Interval = 10000,
                };
                timer.Tick += (sender, e) =>
                {
                    Application.ExitThread();
                };
                timer.Start();

                Application.Run();
            }
        }
        void documentCompleted(
            object sender, 
            WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = sender as WebBrowser;
            if (browser.Url.BaseUrl() != e.Url.BaseUrl()) return;

            // Retrieve the document object once it has finished loading.
            source = getSourceFromBrowser(browser);

            if (showBrowser) return;
            Application.ExitThread();
        }

        static string getSourceFromBrowser(WebBrowser browser)
        {
            dynamic dom = browser.Document?.DomDocument;
            return dom?.documentElement.innerHTML;
        }
    }
}