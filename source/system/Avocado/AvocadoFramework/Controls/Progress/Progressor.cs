using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls.Progress
{
    public class Progressor : Control
    {
        static Progressor()
        {
            // Associate this control with the default theme.
            var type = typeof(Progressor);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(Progressor),
                new FrameworkPropertyMetadata());

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(Progressor),
                new FrameworkPropertyMetadata());

        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
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
            get { return GetValue(StatusProperty) as string; }
            set { SetValue(StatusProperty, value); }
        }
    }
}