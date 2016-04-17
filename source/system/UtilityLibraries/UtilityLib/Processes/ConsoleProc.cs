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
            => RunCriticalCode(() => { action(); return default(object); });

        public static T RunCriticalCode<T>(Func<T> func)
        {
            try { return func(); }
            catch (Exception exc)
            {
                TerminatingError(exc.Message);
                return default(T);
            }
        }
    }
}