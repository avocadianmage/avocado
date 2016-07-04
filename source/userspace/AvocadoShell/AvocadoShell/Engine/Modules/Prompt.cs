using System.Windows.Documents;

namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell { get; private set; }
        public int LengthInSymbols { get; private set; }

        public void Update(bool fromShell, int lengthInSymbols)
        {
            FromShell = fromShell;
            LengthInSymbols = lengthInSymbols;
        }
    }
}