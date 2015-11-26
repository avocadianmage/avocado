using System;

namespace UtilityLib.Processes
{
    public static class ConsoleProc
    {
        public static void TerminatingError(string msg)
        {
            Console.Error.WriteLine(msg);
            Environment.Exit(0);
        }

        public static void RunCriticalCode(Action action)
        {
            try { action.Invoke(); }
            catch (Exception e) { TerminatingError(e.Message); }
        }
    }
}