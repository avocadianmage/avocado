using AvocadoLib.CommandLine.ANSI;
using AvocadoServer.Jobs;
using AvocadoServer.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        static readonly Color errorColor = Colors.Salmon;
        static readonly Color jobColor = Colors.SkyBlue;
        static readonly Dictionary<ClientType, Color> clientTypeColorMapping
            = new Dictionary<ClientType, Color>() {
                { ClientType.ThisMachine, Colors.LightGreen },
                { ClientType.LAN, Colors.Yellow },
                { ClientType.External, Colors.Orange }
            };

        public static void WriteLine(string msg)
            => logLine(false, null, null, msg);

        public static void WriteErrorLine(string msg)
            => logLine(true, null, null, msg);

        public static void WriteLine(Job job, string msg)
            => logLine(false, jobColor, job.ToString(), msg);

        public static void WriteErrorLine(Job job, string msg)
            => logLine(true, jobColor, job.ToString(), msg);

        public static void WriteLine(MethodBase method, params object[] values)
            => logLine(false, method, values);

        public static void WriteErrorLine(
            MethodBase method, 
            params object[] values)
            => logLine(true, method, values);

        static void logLine(
            bool error, MethodBase method, params object[] values)
        {
            var sourceColor 
                = clientTypeColorMapping[ClientIdentifier.GetClientType()];

            var valueQueue = new Queue<object>(values);
            var keyValueStrs = method
                .GetParameters()
                .Select(p => $"{p.Name}: {valueQueue.Dequeue()}");
            var argListStr = $"{{ {string.Join(", ", keyValueStrs)} }}";
            var msg = $"Submitted {method.Name} {argListStr}";

            logLine(error, sourceColor, ClientIdentifier.GetIP(), msg);
        }

        static void logLine(
            bool error, Color? sourceColor, string source, string msg)
        {
            // Don't log an empty message.
            if (string.IsNullOrWhiteSpace(msg)) return;

            // Get timestamp.
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");

            // Format source display text.
            if (sourceColor.HasValue)
            {
                source = ANSICode.GetColoredText(sourceColor.Value, source);
                source = $"({source}) ";
            }

            // Color the message text if it is an error.
            if (error) msg = ANSICode.GetColoredText(errorColor, msg);

            Console.WriteLine($"[{timestamp}] {source}{msg}");
        }

        public static void Log(this Pipeline pipeline)
        {
            var writeAction = pipeline.Success
                ? (Action<string>)WriteLine : WriteErrorLine;
            writeAction(pipeline.Message);
        }
    }
}