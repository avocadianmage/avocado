using System;

namespace AvocadoUtilities
{
    public static class Command
    {
        public static string GetArg(int index)
        {
            index++;
            var args = Environment.GetCommandLineArgs();
            return (index < args.Length) ? args[index] : null;
        }
    }
}