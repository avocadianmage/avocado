using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace AvocadoShell.PowerShellService.Utilities
{
    sealed class Autocomplete
    {
        readonly PowerShell powerShell;

        CommandCompletion completions;
        string expectedInput;
        int expectedIndex;

        public Autocomplete(Runspace runspace)
        {
            powerShell = PowerShell.Create();
            powerShell.Runspace = runspace;
        }

        public (int replacementIndex, 
                int replacementLength, 
                string completionText)? 
            GetCompletion(string input, int index, bool forward)
        {
            // If no input, instruct autocomplete to cycle through the file
            // system entries in the working directory as a default behavior.
            if (string.IsNullOrWhiteSpace(input))
            {
                input = @".\";
                index = input.Length;
            }

            // Check the user has altered the input and we need to update our
            // completion list.
            if (completionListNeedsUpdate(input, index))
            {
                completions = CommandCompletion.CompleteInput(
                    input, index, null, powerShell);
            }

            // Determine the length of the text to replace with the new
            // completion.
            var replacementLength = getCurrentReplacementLength(completions);

            // Get the completion data.
            var result = completions.GetNextResult(forward);
            if (result == null) return null;
            
            // Set input and index of the new completion.
            var replacementIndex = completions.ReplacementIndex;
            var completionText = result.CompletionText;
            expectedInput = input
                .Remove(replacementIndex, replacementLength)
                .Insert(replacementIndex, completionText);
            expectedIndex = replacementIndex + completionText.Length;
            return (replacementIndex, replacementLength, completionText);
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