namespace AvocadoShell.Terminal.Modules
{
    sealed class OutputBuffer
    {
        string whitespaceBuffer;
        bool hitNonWhitespace;

        public OutputBuffer()
        {
            Reset();
        }

        public bool ProcessNewOutput(ref string text)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    if (hitNonWhitespace) whitespaceBuffer += text;
                    return false;
                }

                hitNonWhitespace = true;
                text = whitespaceBuffer + text;
                whitespaceBuffer = string.Empty;

                return true;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                whitespaceBuffer = string.Empty;
                hitNonWhitespace = false;
            }
        }
    }
}