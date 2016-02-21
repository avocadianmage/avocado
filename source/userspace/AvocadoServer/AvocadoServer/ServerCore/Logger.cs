using AvocadoServer.Jobs;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using UtilityLib.WCF;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        static readonly Dictionary<ClientType, Color> clientTypeColorMapping
            = new Dictionary<ClientType, Color>() {
                { ClientType.ThisMachine, Colors.LightGreen },
                { ClientType.LAN, Colors.Yellow },
                { ClientType.Outside, Colors.Orange }
            };

        public static void LogLine(this TextWriter writer, string msg)
            => writer.logLine(null, "sys", msg);

        public static void LogLine(this TextWriter writer, Job job, string msg)
            => writer.logLine(Colors.SkyBlue, job.ToString(), msg);

        public static void LogLine(
            this TextWriter writer, MethodBase method, params object[] values)
        {
            var color 
                = clientTypeColorMapping[ClientIdentifier.GetClientType()];

            var valueQueue = new Queue<object>(values);
            var keyValueStrs = method
                .GetParameters()
                .Select(p => $"{p.Name}: {valueQueue.Dequeue()}");
            var argListStr = $"{{ {string.Join(", ", keyValueStrs)} }}";
            var msg = $"Query: {method.Name} {argListStr}";

            writer.logLine(color, ClientIdentifier.GetIP(), msg);
        }

        static void logLine(
            this TextWriter writer, Color? color, string source, string msg)
        {
            // Don't log an empty message.
            if (string.IsNullOrWhiteSpace(msg)) return;

            // Get timestamp.
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
            
            // Set text color depending on source (job, IP, etc.)
            if (color.HasValue)
            {
                source
                    = ANSICode.GetColorPrefix(color.Value)
                    + source
                    + ANSICode.GetColorPrefix(Colors.LightGray);
            }

            // Use the specified TextWriter to perform the write.
            writer.WriteLine($"{timestamp} [{source}] {msg}");
        }

        public static void WriteWCFMessage(WCFMessage msg)
        {
            var writer = msg.Success ? Console.Out : Console.Error;
            writer.LogLine(msg.Message);
        }
    }
}