using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UtilityLib.Processes;

namespace AvocadoUtilities.CommandLine
{
    public static class Subcommand
    {
        const string SUB_CMD_REQ = "Sub-command required.";
        const string SUB_CMD_INVALID_FMT = "Sub-command {0} is invalid.";
        const string SUB_CMD_ERR_FMT = "{0} Expected: Server [{1}]";

        public static string Invoke()
        {
            var subcommands = getSubcommands();

            // Retrieve the sub-command from the command line input.
            var arg = EnvUtils.GetArg(0);

            // Check for case of no input.
            if (arg == null)
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
            //var subcommandArgs = EnvUtils.GetArgs(1).ToArray();
            var subcommandArgs = new Arguments(EnvUtils.GetArgs(1));
            method.Invoke(null, new object[] { subcommandArgs });
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
                .Where(
                    x => x.GetCustomAttributes<SubcommandAttribute>().Any());
        }

        static string getSubcommandError(
            IEnumerable<MethodInfo> subcommands,
            string message)
        {
            var subcommandNames = subcommands.Select(x => x.Name);
            var subcommandsStr = string.Join("|", subcommandNames);
            return string.Format(SUB_CMD_ERR_FMT, message, subcommandsStr);
        }
    }
}