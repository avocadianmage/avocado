using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls.Selection
{
    public class OptionItem : CheckBox
    {
        static OptionItem()
        {
            // Associate this control with the default theme.
            var type = typeof(OptionItem);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        public static readonly DependencyProperty SingleSelectProperty
            = DependencyProperty.Register(
                "SingleSelect",
                typeof(bool),
                typeof(OptionItem),
                new FrameworkPropertyMetadata());

        public bool SingleSelect
        {
            get { return (bool)GetValue(SingleSelectProperty); }
            set { SetValue(SingleSelectProperty, value); }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (SingleSelect) IsChecked = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (SingleSelect) IsChecked = false;
        }
    }
}