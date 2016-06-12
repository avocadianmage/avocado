using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UtilityLib.MiscTools;

namespace AvocadoUtilities.Context
{
    sealed class ConfigData
    {
        const string DELIM = "=";

        Dictionary<string, string> cache;
        
        readonly string path;

        public ConfigData(string path)
        {
            this.path = path;
        }

        public async Task<string> GetValue(string prop, string defaultVal)
        {
            // Initialize cache, if needed.
            cache = cache ?? await getPropertyDict();

            // Check if the value is already cached.
            if (cache.ContainsKey(prop)) return cache[prop];

            // Otherwise, the config file did not exist, or the property was
            // not found. Create a new file if needed and add the property with
            // its default value.
            var lines = $"{prop}{DELIM}{defaultVal}".Yield();
            await Task.Run(() => File.AppendAllLines(path, lines));

            // Cache the property/value for fast subsequent access.
            cache[prop] = defaultVal;

            return defaultVal;
        }

        async Task<Dictionary<string, string>> getPropertyDict()
        {
            var ret = new Dictionary<string, string>();

            if (!File.Exists(path)) return ret;

            var lines = await Task.Run(() => File.ReadLines(path));
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