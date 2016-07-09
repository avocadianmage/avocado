using System.Collections.Generic;
using System.IO;
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

        public string GetValue(string prop, string defaultVal)
        {
            // Initialize cache, if needed.
            cache = cache ?? getPropertyDict();

            // Check if the value is already cached.
            if (cache.ContainsKey(prop)) return cache[prop];

            // Otherwise, the config file did not exist, or the property was
            // not found. Create a new file if needed and add the property with
            // its default value.
            File.AppendAllLines(path, $"{prop}{DELIM}{defaultVal}".Yield());

            // Cache the property/value for fast subsequent access.
            cache[prop] = defaultVal;

            return defaultVal;
        }

        Dictionary<string, string> getPropertyDict()
        {
            var ret = new Dictionary<string, string>();

            if (!File.Exists(path)) return ret;
            
            foreach (var line in File.ReadLines(path))
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