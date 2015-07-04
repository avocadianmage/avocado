using System.Windows.Media;

namespace AvocadoFramework.Controls.Text
{
    struct Segment
    {        
        public string Text { get { return text; } }
        public Brush Foreground { get { return foreground; } }
        
        readonly string text;
        readonly Brush foreground;

        public Segment(string text, Brush foreground)
        {
            this.text = text;
            this.foreground = foreground;
        }
    }
}