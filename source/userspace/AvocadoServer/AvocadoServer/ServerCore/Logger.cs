using System;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void WriteServerStarted()
        {
            Console.WriteLine(
                "AvocadoServer is now operational at {0}...",
                ServerConfig.BaseAddress);
        }

        public static void WriteLogItem(string msgFmt, params string[] args)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.ff");
            var message = string.Format(msgFmt, args);
            Console.Write("[{0}] {1}", timestamp, message);
        }
    }
}