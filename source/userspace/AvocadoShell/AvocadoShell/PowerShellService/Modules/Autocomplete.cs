using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService.Modules
{
    sealed class Autocomplete
    {
        const string PATH_BEGIN = ".\\";

        readonly PowerShell powerShell;

        CommandCompletion completions;
        string expectedInput;
        int expectedIndex;

        public Autocomplete(Runspace runspace)
        {
            powerShell = PowerShell.Create();
            powerShell.Runspace = runspace;
        }

        public bool GetCompletion(
            string input, int index, bool forward, 
            out int replacementIndex, 
            out int replacementLength, 
            out string completionText)
        {
            replacementIndex = default(int);
            replacementLength = default(int);
            completionText = default(string);

            // Suggest a file if the input is blank.
            if (string.IsNullOrWhiteSpace(input))
            {
                input = PATH_BEGIN;
                index = PATH_BEGIN.Length;
            }

            if (index == 0) return false;

            // Check the user has altered the input and we need to update our
            // completion list.
            if (completionListNeedsUpdate(input, index))
            {
                completions = CommandCompletion.CompleteInput(
                    input, index, null, powerShell);
            }

            // Determine the length of the text to replace with the new
            // completion.
            replacementLength = getCurrentReplacementLength(completions);

            // Get the completion data.
            var result = completions.GetNextResult(forward);
            if (result == null) return false;
            
            // Set input and index of the new completion.
            replacementIndex = completions.ReplacementIndex;
            completionText = result.CompletionText;
            expectedInput = input
                .Remove(replacementIndex, replacementLength)
                .Insert(replacementIndex, completionText);
            expectedIndex = replacementIndex + completionText.Length;
            return true;
        }

        int getCurrentReplacementLength(CommandCompletion completions)
        {
            var matchIndex = completions.CurrentMatchIndex;
            return matchIndex == -1
                ? completions.ReplacementLength
                : completions.CompletionMatches[matchIndex]
                    .CompletionText.Length;
        }

        bool completionListNeedsUpdate(string input, int index) 
            => completions == null 
                || expectedInput != input 
                || expectedIndex != index;
    }
}