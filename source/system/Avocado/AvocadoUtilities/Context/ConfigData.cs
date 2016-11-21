using StandardLibrary.Utilities;
using System.IO;

namespace AvocadoUtilities.Context
{
    public sealed class ConfigData
    {
        const string SECTION = "General";

        readonly ConfigFile configFile;

        public ConfigData(string name)
        {
            var path = Path.Combine(AvocadoContext.AppDataPath, $"{name}.ini");
            configFile = new ConfigFile(path);
        }

        public string Read(string key) => Read(key, string.Empty);

        public string Read(string key, string defaultValue)
        {
            // Attempt to retrieve the value with no defaulting.
            var result = configFile.Read(SECTION, key, string.Empty);
            if (result != string.Empty) return result;

            // If it does not exist, write the default to file and return it.
            configFile.Write(SECTION, key, defaultValue);
            return defaultValue;
        }

        public void Write(string key, string value)
            => configFile.Write(SECTION, key, value);
    }
}