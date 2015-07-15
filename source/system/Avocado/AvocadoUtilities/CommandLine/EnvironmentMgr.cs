using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoUtilities.CommandLine
{
    public static class EnvironmentMgr
    {
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

        public static void TerminatingError(string msg, params string[] args)
        {
            Console.Error.WriteLine(msg, args);
            Environment.Exit(0);
        }
    }
}