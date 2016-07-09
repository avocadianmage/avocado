using System.IO;
using System.Reflection;

namespace AvocadoUtilities.Context
{
    public sealed class AvocadoContext
    {
        readonly ConfigData configData = new ConfigData("config.ini");

        public void Establish(Assembly asm)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(asm.Location));
        }

        public string GetConfigValue(string prop, string defaultVal)
            => configData.GetValue(prop, defaultVal);
    }
}