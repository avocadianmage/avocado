using System.Windows.Media;

namespace AvocadoUtilities.CommandLine.ANSI
{
    public struct ANSISegment
    {
        public Color? Color { get; }
        public string Text { get; }

        public ANSISegment(Color? color, string text)
        {
            Color = color;
            Text = text;
        }
    }
}