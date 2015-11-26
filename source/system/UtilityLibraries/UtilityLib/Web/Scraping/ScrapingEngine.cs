using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UtilityLib.Web.Scraping
{
    public sealed class ScrapingEngine
    {
        public async Task<string> GetSource(string url)
        {
            return await GetSource(url, false);
        }

        public async Task<string> GetSource(string url, bool onlyDynamic)
        {
            if (onlyDynamic)
            {
                var scraper = new DynamicScaper();
                return await scraper.GetSource(url);
            }

            return await Task.Run(() => dispatchGetSource(url));
        }

        string dispatchGetSource(string url)
        {
            var tasks = getScraperTypes()
                .Select(type => getSourceTask(type, url))
                .ToList();

            while (tasks.Any())
            {
                // Get the result of the first task to complete.
                var index = Task.WaitAny(tasks.ToArray());
                var result = tasks[index].Result;

                // If the task was successful (the result is not null) then 
                // return the data.
                if (result != null) return result;

                // Otherwise, remove the failed task and continue.
                tasks.RemoveAt(index);
            }

            // Throw an exception if all of the tasks failed.
            throw new Exception("Operation failed.");
        }

        IEnumerable<Type> getScraperTypes()
        {
            Func<Type, bool> isScraperType = t =>
            {
                return typeof(IScraper).IsAssignableFrom(t) && !t.IsInterface;
            };
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => isScraperType(t));
        }

        Task<string> getSourceTask(Type type, string url)
        {
            var scraper = Activator.CreateInstance(type) as IScraper;
            return Task.Run(() => scraper.GetSource(url));
        }
    }
}