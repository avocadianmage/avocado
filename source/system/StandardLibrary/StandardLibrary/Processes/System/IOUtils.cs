using System.IO;
using System.Runtime.InteropServices;

namespace StandardLibrary.Processes.System
{
    public static class IOUtils
    {
        public static bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                return errorCode == 32 || errorCode == 33;
            }
            return false;
        }
    }
}