using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AvocadoLib.CommandLine.Arguments
{
    public static class Subcommand
    {
        const string SUB_CMD_REQ = "Sub-command required.";
        const string SUB_CMD_INVALID_FMT = "Sub-command {0} is invalid.";
        const string SUB_CMD_ERR_FMT = "{0} Expected: {1} [{2}]";

        public static string Invoke()
        {
            var subcommands = getSubcommands();
            var commandLineArgs = Environment.GetCommandLineArgs().Skip(1);

            // Retrieve the sub-command from the command line input.
            var arg = commandLineArgs.FirstOrDefault();

            // Check for case of no input.
            if (arg == default(string))
            {
                return getSubcommandError(subcommands, SUB_CMD_REQ);
            }

            // Retrieve the sub-command method to execute.
            var method = findSubcommand(subcommands, arg);

            // Check for case of no sub-command found.
            if (method == null)
            {
                var msg = string.Format(SUB_CMD_INVALID_FMT, arg);
                return getSubcommandError(subcommands, msg);
            }

            // Invoke the sub-command.
            method.Invoke(null, new object[] { commandLineArgs.Skip(1) });
            return null;
        }

        static MethodInfo findSubcommand(
            IEnumerable<MethodInfo> subcommands,
            string search)
        {
            return subcommands.FirstOrDefault(
                x => x.Name.Equals(
                    search,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        static IEnumerable<MethodInfo> getSubcommands()
        {
            return Assembly
                .GetEntryAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes<SubcommandAttribute>().Any());
        }

        static string getSubcommandError(
            IEnumerable<MethodInfo> subcommands,
            string message)
        {
            var subcommandNames = subcommands.Select(x => x.Name);
            var subcommandsStr = string.Join("|", subcommandNames);
            return string.Format(
                SUB_CMD_ERR_FMT,
                message,
                Process.GetCurrentProcess().ProcessName,
                subcommandsStr);
        }
    }
}