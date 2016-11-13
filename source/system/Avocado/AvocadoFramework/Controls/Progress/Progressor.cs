using AvocadoFramework.Animation;
using StandardLibrary.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvocadoFramework.Controls.Progress
{
    public class Progressor : Control
    {
        ReversibleAnimator<Color> borderColorAnimator;
        ReversibleAnimator<Color> textColorAnimator;

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
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
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

        public Progressor()
        {
            FocusVisualStyle = null;
            GotFocus += (sender, e) => animateSelection(true);
            LostFocus += (sender, e) => animateSelection(false);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            initAnimation();
        }

        void animateSelection(bool selected)
        {
            borderColorAnimator.Animate(selected);
            textColorAnimator.Animate(selected);
        }

        void initAnimation()
        {
            borderColorAnimator = new ReversibleAnimator<Color>(
                SolidColorBrush.ColorProperty,
                Config.ProgressorColor,
                Config.ProgressorSelectedColor,
                Config.ProgressorFadeDuration,
                this.GetTemplateElement<Border>("border").BorderBrush);

            textColorAnimator = new ReversibleAnimator<Color>(
                SolidColorBrush.ColorProperty,
                Config.ProgressorTextColor,
                Config.ProgressorSelectedColor,
                Config.ProgressorFadeDuration,
                this.GetTemplateElement<Label>("title").Foreground,
                this.GetTemplateElement<Label>("status").Foreground);
        }
    }
}