using System;
using UtilityLib.Web.Scraping;

namespace UtilityLibTest
{
    static class ScraperTest
    {
        static void Main(string[] args)
        {
            var url = args[0];
            var source = new DynamicScaper().GetSource(url).Result;
            Console.WriteLine(source);
            Console.ReadLine();
        }
    }
}