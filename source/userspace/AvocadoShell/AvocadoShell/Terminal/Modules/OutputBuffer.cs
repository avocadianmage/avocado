using System.Windows.Media;

namespace AvocadoShell.Terminal.Modules
{
    sealed class OutputBuffer
    {
        string newlineBuffer;
        bool hitNonwhitespace;

        public OutputBuffer()
        {
            Reset();
        }

        public bool ProcessNewOutput(ref string text, ref Brush brush)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    if (hitNonwhitespace) newlineBuffer += "\r";
                    return false;
                }

                hitNonwhitespace = true;
                text = newlineBuffer + text;
                newlineBuffer = string.Empty;

                return true;
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