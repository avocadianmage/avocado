﻿using System;
using System.Collections.Generic;

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

        public static T RunCriticalCode<T>(Func<T> action)
        {
            try { return action.Invoke(); }
            catch (Exception e)
            {
                TerminatingError(e.Message);
                return default(T);
            }
        }
    }
}