namespace AvocadoShell.Engine.Modules
{
    sealed class Prompt
    {
        public bool FromShell { get; private set; }
        public string Text { get; private set; }

        public void Update(bool fromShell, string text)
        {
            FromShell = fromShell;
            Text = text;
        }
    }
}