using AvocadoLib.CommandLine.Arguments;
using System;

namespace AvocadoCommClient
{
    sealed class Program
    {
        static void Main()
        {
            var error = Subcommand.Invoke();
            if (error != null) Console.Error.WriteLine(error);
        }
    }
}