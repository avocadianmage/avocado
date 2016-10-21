using System.Threading.Tasks;

namespace StandardLibrary.Web.Scraping
{
    public interface IScraper
    {
        Task<string> GetSource(string url);
    }
}