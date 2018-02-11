using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls.Progress
{
    public class Progressor : ProgressBar
    {
        static Progressor()
        {
            // Associate this control with the default theme.
            var type = typeof(Progressor);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(Progressor),
                new FrameworkPropertyMetadata());

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty
            = DependencyProperty.Register(
                "Status",
                typeof(string),
                typeof(Progressor),
                new FrameworkPropertyMetadata());

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }
    }
}