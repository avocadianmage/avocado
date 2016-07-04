using AvocadoFramework.Animation;
using System.Windows.Media;

namespace AvocadoShell.Engine.Modules
{
    sealed class OutputBuffer
    {
        public static Brush CreateTextFadeBrush(Brush baseBrush)
        {
            return new BrushAnimation().GetFadingBrush(
                baseBrush,
                Config.TextFadeDuration);
        }

        string newlineBuffer;
        bool hitNonwhitespace;
        Brush animatedBrush;

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

                brush = animatedBrush = 
                    animatedBrush ?? CreateTextFadeBrush(brush);

                return true;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                newlineBuffer = string.Empty;
                hitNonwhitespace = false;
                animatedBrush = null;
            }
        }
    }
}