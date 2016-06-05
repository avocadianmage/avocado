using System.Management.Automation;
using System.Threading.Tasks;

namespace AvocadoShell.PowerShellService.Modules
{
    sealed class Autocomplete
    {
        const string PATH_BEGIN = ".\\";

        readonly PowerShell ps;

        CommandCompletion cachedCompletions;
        string expectedInput;
        int expectedInputIndex;

        public Autocomplete(PowerShell ps)
        {
            this.ps = ps;
        }

        public async Task InitializeService()
        {
            const string DUMMY = "a";
            await Task.Run(() => CommandCompletion.CompleteInput(
                DUMMY, DUMMY.Length, null, ps));
        }

        public async Task<string> GetCompletion(
            string input, int index, bool forward)
            => await Task.Run(() => getCompletion(input, index, forward));

        string getCompletion(string input, int index, bool forward)
        {
            // Suggest a file if the input is blank.
            if (string.IsNullOrWhiteSpace(input))
            {
                input = PATH_BEGIN;
                index = PATH_BEGIN.Length;
            }

            if (index == 0) return null;

            // Check the user has altered the input and we need to update our
            // completion list.
            if (completionListNeedsUpdate(input, index))
            {
                cachedCompletions = CommandCompletion.CompleteInput(
                    input, index, null, ps);
            }

            var result = cachedCompletions.GetNextResult(forward);
            if (result == null) return null;

            expectedInput = input.Substring(
                0, cachedCompletions.ReplacementIndex);
            expectedInput += result.CompletionText;
            expectedInputIndex = expectedInput.Length;
            return expectedInput;
        }

        bool completionListNeedsUpdate(string input, int inputIndex)
        {
            if (cachedCompletions == null) return true;
            if (expectedInput != input) return true;
            if (expectedInputIndex != inputIndex) return true;
            return false;
        }
    }
}