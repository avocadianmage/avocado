namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell { get; }
        public string Text { get; }
    
        public Prompt(bool fromShell, string text)
        {
            FromShell = fromShell;
            Text = text;
        }
    }
}