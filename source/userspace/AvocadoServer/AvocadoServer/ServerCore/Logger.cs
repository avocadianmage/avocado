using AvocadoServer.Jobs;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.IO;
using System.Windows.Media;
using UtilityLib.WCF;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void WriteLine(string msg) => WriteLine(null, msg);

        public static void WriteErrorLine(string msg)
            => WriteErrorLine(null, msg);

        public static void WriteLine(Job job, string msg)
        {
            // If stdout text is being written from a job, give it a distinct
            // color.
            if (job != null)
            {
                msg = ANSICode.GetColoredText(Brushes.SkyBlue, msg);
            }

            writeLine(Console.Out, job, msg);
        }

        public static void WriteErrorLine(Job job, string msg)
            => writeLine(Console.Error, job, msg);

        static void writeLine(TextWriter writer, Job job, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
            var jobStr = job?.ToString() ?? "sys";
            writer.WriteLine($"{timestamp} [{jobStr}] {msg}");
        }

        public static void WriteWCFMessage(WCFMessage msg)
        {
            var writer = msg.Success
                ? (Action<string>)WriteLine
                : WriteErrorLine;
            writer(msg.Message);
        }
    }
}