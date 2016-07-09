using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UtilityLib.Web.Scraping
{
    public sealed class ScrapingEngine
    {
        public Task<string> GetSource(string url) => GetSource(url, false);

        public Task<string> GetSource(string url, bool onlyDynamic)
        {
            return onlyDynamic
                ? new DynamicScaper().GetSource(url)
                : dispatchGetSource(url);
        }

        async Task<string> dispatchGetSource(string url)
        {
            var tasks = getScraperTypes()
                .Select(type => getSourceTask(type, url))
                .ToList();

            while (tasks.Any())
            {
                // Get the result of the first task to complete.
                var task 
                    = await Task.WhenAny(tasks.ToArray()).ConfigureAwait(false);

                // If the task was successful (the result is not null) then 
                // return the data.
                var result = await task.ConfigureAwait(false);
                if (result != null) return result;

                // Otherwise, remove the failed task and continue.
                tasks.Remove(task);
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
            => (Activator.CreateInstance(type) as IScraper).GetSource(url);
    }
}