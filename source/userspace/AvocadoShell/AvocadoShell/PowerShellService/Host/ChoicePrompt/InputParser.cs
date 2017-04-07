using System;
using System.Collections.Generic;
using System.Linq;

namespace AvocadoShell.PowerShellService.Host.ChoicePrompt
{
    static class InputParser
    {
        static string getSanitizedInput(Func<string> getInput)
            => getInput().Trim().ToUpper();

        public static int SingleChoicePrompt(
            Func<string> getInput, Choice[] choiceList, int defaultChoice)
        {
            var input = getSanitizedInput(getInput);

            // If the choice string was empty, use the default selection.
            if (string.IsNullOrEmpty(input) && defaultChoice != -1)
            {
                return defaultChoice;
            }

            // See if the selection matched and return the corresponding
            // index if it did.
            for (var i = 0; i < choiceList.Length; i++)
            {
                var choice = choiceList[i];
                if (choice.Hotkey == input || choice.Text.ToUpper() == input)
                {
                    return i;
                }
            }

            // Otherwise, the input was invalid. Prompt again.
            return SingleChoicePrompt(getInput, choiceList, defaultChoice);
        }

        public static IEnumerable<int> MultiChoiceNumericPrompt(
            Func<string> getInput, 
            int optionsCount, 
            IEnumerable<int> defaultOptions)
        {
            var input = getSanitizedInput(getInput);

            // Return the default options if nothing was entered.
            if (string.IsNullOrEmpty(input) && defaultOptions.Any())
            {
                return defaultOptions;
            }

            var rangeList = input.Split(
                new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries);
            if (!rangeList.Any())
            {
                return MultiChoiceNumericPrompt(
                    getInput, optionsCount, defaultOptions);
            }

            var selections = new List<int>();
            foreach (var range in rangeList)
            {
                var bounds = range.Split(
                    new char[] { ':' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (bounds.Length < 1 || bounds.Length > 2)
                {
                    return MultiChoiceNumericPrompt(
                        getInput, optionsCount, defaultOptions);
                }

                if (!validateSingleSelection(
                    bounds[0],
                    optionsCount,
                    out var lowerBound))
                {
                    return MultiChoiceNumericPrompt(
                        getInput, optionsCount, defaultOptions);
                }
                lowerBound--;

                if (bounds.Length == 1)
                {
                    selections.Add(lowerBound);
                    continue;
                }

                if (!validateSingleSelection(
                    bounds[1],
                    optionsCount,
                    out var upperBound))
                {
                    return MultiChoiceNumericPrompt(
                        getInput, optionsCount, defaultOptions);
                }
                upperBound--;

                if (lowerBound > upperBound)
                {
                    return MultiChoiceNumericPrompt(
                        getInput, optionsCount, defaultOptions);
                }

                selections.AddRange(Enumerable.Range(0, optionsCount)
                    .Skip(lowerBound)
                    .Take(upperBound - lowerBound + 1));
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
    }
}
