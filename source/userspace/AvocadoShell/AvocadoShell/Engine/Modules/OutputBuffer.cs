namespace AvocadoShell.Engine.Modules
{
    sealed class OutputBuffer
    {
        string newlineBuffer;
        bool hitNonwhitespace;

        public OutputBuffer()
        {
            Reset();
        }

        public string ProcessNewOutput(string text)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    if (hitNonwhitespace) newlineBuffer += "\r";
                    return null;
                }

                hitNonwhitespace = true;
                text = newlineBuffer + text;
                newlineBuffer = string.Empty;
                return text;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                newlineBuffer = string.Empty;
                hitNonwhitespace = false;
            }
        }
    }
}