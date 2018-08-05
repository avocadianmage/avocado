using System.Windows.Media;

namespace AvocadoLib.CommandLine.ANSI
{
    public sealed class ANSISegment
    {
        public Color? Color { get; }
        public string Text { get; set; }

        public ANSISegment(Color? color, string text)
        {
            Color = color;
            Text = text;
        }
    }
}