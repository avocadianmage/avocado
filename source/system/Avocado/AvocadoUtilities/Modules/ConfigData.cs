using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvocadoUtilities.Modules
{
    public class ConfigData
    {
        const string DELIM = "=";

        Dictionary<string, string> cache;

        readonly string path;

        public ConfigData(string path)
        {
            this.path = path;
        }

        public async Task<string> GetValue(
            string property,
            string defaultVal)
        {
            // Initialize cache, if needed.
            cache = cache ?? await getPropertyDict();

            // Check if the value is already cached.
            if (cache.ContainsKey(property)) return cache[property];

            // Otherwise, the config file did not exist, or the property was
            // not found. Create a new file if needed and add the property with
            // its default value.
            using (var writer = new StreamWriter(path, true))
            {
                await writer.WriteLineAsync($"{property}{DELIM}{defaultVal}");
            }

            // Cache the property/value for fast subsequent access.
            cache[property] = defaultVal;

            return defaultVal;
        }

        async Task<Dictionary<string, string>> getPropertyDict()
        {
            var ret = new Dictionary<string, string>();

            if (!File.Exists(path)) return ret;

            string fileContents;
            using (var reader = new StreamReader(path))
            {
                fileContents = await reader.ReadToEndAsync();
            }

            var lines = fileContents.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var index = line.IndexOf(DELIM);
                if (index == -1) continue;

                var property = line.Substring(0, index);
                if (string.IsNullOrWhiteSpace(property)) continue;

                var val = line.Substring(index + DELIM.Length);
                if (string.IsNullOrWhiteSpace(val)) continue;

                ret[property] = val;
            }

            return ret;
        }
    }
}