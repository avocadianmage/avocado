using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvocadoUtilities.Modules
{
    public class ConfigData
    {
        readonly Dictionary<string, string> cache 
            = new Dictionary<string, string>();

        public async Task<string> GetValue(
            string property,
            string defaultVal)
        {
            // Check if the value is already cached.
            if (cache.ContainsKey(property)) return cache[property];

            var configFile = Path.Combine(
                RootDir.Avocado.Apps.MyAppDataPath,
                "config.ini");
            var propertyMatch = $"{property}=";

            // Check if config file exists.
            if (File.Exists(configFile))
            {
                using (var reader = new StreamReader(configFile))
                {
                    // Read through each line in the config.
                    string line;
                    do
                    {
                        line = await reader.ReadLineAsync();

                        // If the line matches the target property, return its
                        // associated value.
                        if (line.StartsWith(propertyMatch))
                        {
                            var val = line.Substring(propertyMatch.Length);

                            // Cache the property/value for fast subsequent 
                            // access.
                            cache[property] = val;

                            return val;
                        }
                    }
                    while (line != null);
                }

            }

            // Otherwise, the config file did not exist, or the property was
            // not found. Create a new file if needed and add the property with
            // its default value.
            using (var writer = new StreamWriter(configFile, true))
            {
                await writer.WriteLineAsync(propertyMatch + defaultVal);
            }

            // Cache the property/value for fast subsequent access.
            cache[property] = defaultVal;

            return defaultVal;
        }
    }
}