using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoUtilities.CommandLine
{
    public static class EnvironmentMgr
    {
        public static void TerminatingError(string msg, params string[] args)
        {
            Console.Error.WriteLine(msg, args);
            Environment.Exit(0);
        }
    }
}