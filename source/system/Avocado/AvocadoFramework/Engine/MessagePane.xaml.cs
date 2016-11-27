using System.Windows.Input;

namespace AvocadoFramework.Engine
{
    public partial class MessagePane : GlassPane
    {
        public MessagePane(string message)
        {
            InitializeComponent();
            Title = message;
            Message.Content = message;
        }

        void GlassPane_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Escape:
                    Close();
                    break;
            }
        }
    }
}
