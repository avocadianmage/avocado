using System.Runtime.InteropServices;
using System.Text;

namespace StandardLibrary.Utilities
{
    public class ConfigFile
    {
        string path;

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(
            string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(
            string section, 
            string key, 
            string def, 
            StringBuilder retVal, 
            int size, 
            string filePath);
        
        public ConfigFile(string path)
        {
            this.path = path;
        }

        public void Write(string section, string key, string value)
            => WritePrivateProfileString(section, key, value, path);

        public string Read(string section, string key, string defaultValue)
        {
            const int MAX_SIZE = 255;
            var builder = new StringBuilder(MAX_SIZE);
            GetPrivateProfileString(
                section, key, defaultValue, builder, MAX_SIZE, path);
            return builder.ToString();

        }
    }
}