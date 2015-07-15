using System;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void WriteServerStarted()
        {
            Console.WriteLine(
                "AvocadoServer ({0}) is now running...",
                ServerConfig.BaseAddress);
        }

        public static void WriteLogItem(string msgFmt, params string[] args)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.ff");
            var message = string.Format(msgFmt, args);
            Console.WriteLine("[{0}] {1}", timestamp, message);
        }
    }
}