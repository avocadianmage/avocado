using System;

namespace AvocadoUtilities
{
    public static class Command
    {
        public static string GetArg(int index)
        {
            var args = Environment.GetCommandLineArgs();
            return (args.Length >= index + 1) ? args[index] : null;
        }
    }
}