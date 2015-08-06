using AvocadoServer.Jobs;
using System;
using System.IO;
using UtilityLib.WCF;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void WriteLine(string msg) => WriteLine(null, msg);

        public static void WriteErrorLine(string msg)
            => WriteErrorLine(null, msg);

        public static void WriteLine(Job job, string msg)
            => writeLine(Console.Out, job, msg);

        public static void WriteErrorLine(Job job, string msg)
            => writeLine(Console.Error, job, msg);

        static void writeLine(TextWriter writer, Job job, string msg)
        {
            if (msg == null) return;
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.ff");
            writer.WriteLine(
                $"{timestamp} [{job?.ToString() ?? "sys"}] {msg}");
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