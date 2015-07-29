using System;

namespace AvocadoUtilities.CommandLine
{
    public static class EnvironmentMgr
    {
        public static void TerminatingError(string msg, params string[] args)
        {
            Console.Error.WriteLine(msg, args);
            Environment.Exit(0);
        }

        public static void RunCriticalCode(Action action)
        {
            try { action.Invoke(); }
            catch (Exception e){TerminatingError(e.Message); }
        }
    }
}