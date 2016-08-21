using System.IO;
using System.Reflection;

namespace AvocadoUtilities.Context
{
    public sealed class AvocadoContext
    {
        readonly ConfigData configData;

        public AvocadoContext(Assembly asm, string commonName)
        {
            configData = new ConfigData(commonName);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(asm.Location));
        }

        public string GetConfigValue(string prop, string defaultVal)
            => configData.GetValue(prop, defaultVal);
    }
}