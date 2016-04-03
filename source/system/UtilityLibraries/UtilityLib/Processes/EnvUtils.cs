using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static string CommandName
            => Process.GetCurrentProcess().ProcessName;

        public static string GetArg(int index)
            => Environment.GetCommandLineArgs().ElementAtOrDefault(index + 1);

        public static IEnumerable<string> GetArgs(int startIndex)
            => Environment.GetCommandLineArgs().Skip(startIndex + 1);

        public static string BuildArgStr() 
            => BuildArgStr(GetArgs(0).ToArray());

        public static string BuildArgStr(params string[] args)
        {
            // Escape each argument with quotes.
            return args.Any() 
                ? $"\"{ string.Join("\" \"", args) }\"" : string.Empty;
        }

        public static string[] SplitArgStr(string argStr)
        {
            // Remove first and last quote character from string.
            argStr = argStr.Substring(1, argStr.Length - 2);

            return argStr.Split(
                new string[] { "\" \"" }, 
                StringSplitOptions.None);
        }

        public static string GetFilePath(this string filename)
        {
            if (File.Exists(filename)) return Path.GetFullPath(filename);

            var envPaths = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in envPaths.Split(';'))
            {
                var filePath = Path.Combine(path, filename);
                if (File.Exists(filePath)) return filePath;
            }
            return null;
        }
    }
}
