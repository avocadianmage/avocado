using System.Threading.Tasks;

namespace UtilityLib.Web.Scraping
{
    public interface IScraper
    {
        Task<string> GetSource(string url);
    }
}