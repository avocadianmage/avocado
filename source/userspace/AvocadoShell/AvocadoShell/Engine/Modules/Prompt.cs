namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell { get; private set; }
        public int Length { get; private set; }

        public void Update(bool fromShell, int length)
        {
            FromShell = fromShell;
            Length = length;
        }
    }
}