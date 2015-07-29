using System;

namespace AvocadoServer.ServerCore
{
    static class Logger
    {
        public static void WriteServerStarted(string endpoint)
            => Console.WriteLine(
                $"AvocadoServer is now running at {endpoint}...");

        public static void WriteLine(string msg) => WriteLine("sys", msg);

        public static void WriteLine(string job, string msg)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy HH:mm:ss.ff");
            Console.WriteLine($"{timestamp} [{job}] {msg}");
        }
    }
}