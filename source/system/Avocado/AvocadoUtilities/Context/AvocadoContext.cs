using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AvocadoUtilities.Context
{
    public sealed class AvocadoContext
    {
        readonly ConfigData configData = new ConfigData("config.ini");

        public void Establish(Assembly asm)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(asm.Location));
        }

        public async Task<string> GetConfigValue(
            string prop, string defaultVal)
            => await configData.GetValue(prop, defaultVal);
    }
}