using System;
using System.IO;
using System.Reflection;

namespace AvocadoUtilities.Context
{
    public sealed class AvocadoContext
    {
        public static string AppDataPath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appDataPath, "Avocado");
            }
        }

        public ConfigData Config { get; }

        public AvocadoContext(Assembly asm, string commonName)
        {
            Config = new ConfigData(commonName);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(asm.Location));
        }
    }
}