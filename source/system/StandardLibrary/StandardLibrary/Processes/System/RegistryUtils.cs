using Microsoft.Win32;

namespace StandardLibrary.Processes.System
{
    public static class RegistryUtils
    {
        public static void SetRegistryValue(
            string keyPath, string valueName, object valueData)
        {
            Registry.CurrentUser
                .OpenSubKey(keyPath, true)
                .SetValue(valueName, valueData);
        }
    }
}