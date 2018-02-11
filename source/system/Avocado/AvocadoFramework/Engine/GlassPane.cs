using StandardLibrary.Processes;
using StandardLibrary.WPF;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Engine
{
    public abstract class GlassPane : Window
    {
        public static Color ActiveOutlineColor { get; } 
            = EnvUtils.IsAdmin ? Colors.Salmon : Colors.CornflowerBlue;
        readonly TimeSpan windowFadeDuration = TimeSpan.FromMilliseconds(200);

        Border paneUI => this.GetTemplateElement<Border>("Pane");
        
        bool closeImmediately = false;
        
        static GlassPane()
        {
            // Associate this window with the default theme.
            var frameType = typeof(GlassPane);
            DefaultStyleKeyProperty.OverrideMetadata(
                frameType,
                new FrameworkPropertyMetadata(frameType));
        }

        public static readonly DependencyProperty MaxOpacityProperty
            = DependencyProperty.Register(
                "MaxOpacity",
                typeof(double),
                typeof(GlassPane),
                new FrameworkPropertyMetadata());

        public double MaxOpacity
        {
            get { return (double)GetValue(MaxOpacityProperty); }
            set { SetValue(MaxOpacityProperty, value); }
        }

        public GlassPane()
        {
            Loaded += (s, e) =>
            {
                var animation = new DoubleAnimation(
                    MaxOpacity, windowFadeDuration);
                Task.Delay(1).ContinueWith(
                    t => BeginAnimation(OpacityProperty, animation),
                    TaskScheduler.FromCurrentSynchronizationContext());
            };
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            paneUI.GetResource<Storyboard>("FocusFadeIn").Begin();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            paneUI.GetResource<Storyboard>("FocusFadeOut").Begin();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Drag the window on left click.
            DragMove();
        }

        void applyCloseAnimation()
        {
            var animation = new DoubleAnimation(0, windowFadeDuration);
            animation.Completed += onCloseAnimationComplete;
            BeginAnimation(OpacityProperty, animation);
        }

        void onCloseAnimationComplete(object sender, EventArgs e)
        {
            closeImmediately = true;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (closeImmediately) return;
            e.Cancel = true;
            applyCloseAnimation();
        }
    }
}
