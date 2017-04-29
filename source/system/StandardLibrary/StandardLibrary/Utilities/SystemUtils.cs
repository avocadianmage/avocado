using Microsoft.Win32;

namespace StandardLibrary.Utilities
{
    public static class SystemUtils
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