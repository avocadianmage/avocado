using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace StandardLibrary.Processes
{
    public static class EnvUtils
    {
        public static bool IsAdmin =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

        public static string CommandName
            => Process.GetCurrentProcess().ProcessName;

        public static string GetArg(int index)
            => Environment.GetCommandLineArgs().ElementAtOrDefault(index + 1);

        public static IEnumerable<string> GetArgs() => GetArgs(0);

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

        public static string GetEmbeddedText(Assembly asm, string resourceName)
        {
            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
