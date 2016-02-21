using AvocadoServer.Jobs;
using AvocadoUtilities.CommandLine.ANSI;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows.Media;
using UtilityLib.WCF;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void LogLine(this TextWriter writer, string msg)
            => writer.logLine(null, "sys", msg);

        public static void LogLine(this TextWriter writer, Job job, string msg)
            => writer.logLine(Colors.SkyBlue, job.ToString(), msg);

        public static void LogLine(
            this TextWriter writer, string cmd, Dictionary<string, string> args)
        {
            var argList = args?.ToString() ?? string.Empty;
            var msg = $"Query: {cmd} {argList}";
            writer.logLine(Colors.Yellow, getClientIP(), msg);
        }

        static void logLine(
            this TextWriter writer, Color? color, string source, string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss");
            
            if (color.HasValue)
            {
                source
                    = ANSICode.GetColorPrefix(color.Value)
                    + source
                    + ANSICode.GetColorPrefix(Colors.LightGray);
            }

            writer.WriteLine($"{timestamp} [{source}] {msg}");
        }

        public static void WriteWCFMessage(WCFMessage msg)
        {
            var writer = msg.Success ? Console.Out : Console.Error;
            writer.LogLine(msg.Message);
        }
        
        

        static string getClientIP()
        {
            var prop = OperationContext.Current.IncomingMessageProperties;
            var endpoint = prop[RemoteEndpointMessageProperty.Name];
            return (endpoint as RemoteEndpointMessageProperty).Address;
        }
    }
}