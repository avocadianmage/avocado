using AvocadoClient.ServerAPIReference;
using System;

namespace AvocadoClient
{
    static class Logger
    {
        public static void Log(this Pipeline pipeline)
        {
            var text = pipeline.Message;
            if (string.IsNullOrWhiteSpace(text)) return;
            var writer = pipeline.Success ? Console.Out : Console.Error;
            writer.WriteLine(text);
        }
    }
}