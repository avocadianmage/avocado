using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace UtilityLib.Processes
{
    public static class EnvUtils
    {
        public static bool IsAdmin
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static string GetArg(int index)
        {
            return Environment
                .GetCommandLineArgs()
                .ElementAtOrDefault(index + 1);
        }

        public static IEnumerable<string> GetArgs(int startIndex)
        {
            return Environment.GetCommandLineArgs().Skip(startIndex + 1);
        }

        public static string BuildArgStr()
        {
            return BuildArgStr(GetArgs(0).ToArray());
        }

        public static string BuildArgStr(params string[] args)
        {
            // Escape each argument with quotes.
            return $"\"{ string.Join("\" \"", args) }\"";
        }

        public static string[] SplitArgStr(string argStr)
        {
            // Remove first and last quote character from string.
            argStr = argStr.Substring(1, argStr.Length - 2);

            return argStr.Split(
                new string[] { "\" \"" }, 
                StringSplitOptions.None);
        }
    }
}
