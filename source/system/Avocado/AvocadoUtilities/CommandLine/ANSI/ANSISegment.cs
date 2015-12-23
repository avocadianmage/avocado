using System.Windows.Media;

namespace AvocadoUtilities.CommandLine.ANSI
{
    public struct ANSISegment
    {
        public Brush Brush { get; }
        public string Text { get; }

        public ANSISegment(Brush brush, string text)
        {
            Brush = brush;
            Text = text;
        }
    }
}