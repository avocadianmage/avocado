using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace AvocadoUtilities.Cmdlet
{
    public static class Prompts
    {
        public static string TextPrompt(this PSCmdlet cmdlet, string msg)
        {
            var col = new Collection<FieldDescription>
            {
                new FieldDescription(msg),
            };
            var result = cmdlet.Host.UI.Prompt(null, null, col);
            return result[msg].ToString();
        }

        public static T OptionPrompt<T>(
            this PSCmdlet cmdlet, 
            params T[] options)
        {
            var input = cmdlet.TextPrompt("Input selection");

            // Return null if nothing was entered.
            if (string.IsNullOrWhiteSpace(input)) return default(T);

            if (!validateSingleSelection(
                input,
                options.Length,
                out var selection))
            {
                cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                return OptionPrompt(cmdlet, options);
            }
            selection--;

            return options[selection];
        }

        public static IEnumerable<T> MultiOptionPrompt<T>(
            this PSCmdlet cmdlet, 
            params T[] options)
        {
            var input = cmdlet.TextPrompt("Input selection(s)");

            // Return null if nothing was entered.
            if (string.IsNullOrWhiteSpace(input)) return null;

            var rangeList = input.Split(
                new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);
            if (!rangeList.Any())
            {
                cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                return MultiOptionPrompt(cmdlet, options);
            }

            var selections = new List<T>();

            foreach (var range in rangeList)
            {
                var bounds = range.Split(
                    new char[] { ':' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (bounds.Length < 1 || bounds.Length > 2)
                {
                    cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                    return MultiOptionPrompt(cmdlet, options);
                }

                if (!validateSingleSelection(
                    bounds[0],
                    options.Length,
                    out var lowerBound))
                {
                    cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                    return MultiOptionPrompt(cmdlet, options);
                }
                lowerBound--;

                if (bounds.Length == 1)
                {
                    selections.Add(options[lowerBound]);
                    continue;
                }

                if (!validateSingleSelection(
                    bounds[1],
                    options.Length,
                    out var upperBound))
                {
                    cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                    return MultiOptionPrompt(cmdlet, options);
                }
                upperBound--;

                if (lowerBound > upperBound)
                {
                    cmdlet.Host.UI.WriteErrorLine("Invalid input.");
                    return MultiOptionPrompt(cmdlet, options);
                }

                var elems = options
                    .Skip(lowerBound)
                    .Take(upperBound - lowerBound + 1);
                selections.AddRange(elems);
            }

            return selections.Distinct();
        }

        static bool validateSingleSelection(
            string input, 
            int count,
            out int selection)
        {
            return int.TryParse(input, out selection)
                && selection >= 1
                && selection <= count;
        }

        public static void FormatOptions(
            this PSCmdlet cmdlet, 
            params string[] options)
        {
            var padding = options.Length.ToString().Length;
            for (var i = 0; i < options.Length; i++)
            {
                var fmtNum = (i + 1).ToString().PadLeft(padding);
                var opt = $" ({fmtNum}) {options[i]}";
                cmdlet.WriteObject(opt);
            }
        }
    }
}