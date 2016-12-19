using StandardLibrary.Web.Scraping;
using System;

namespace StandardLibraryTest
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