using StandardLibrary.Processes;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Engine
{
    public abstract class GlassPane : Window
    {
        readonly Color activeBorderColor
            = EnvUtils.IsAdmin ? Colors.Salmon : Colors.CornflowerBlue;
        readonly TimeSpan windowFadeDuration = TimeSpan.FromMilliseconds(200);
        readonly TimeSpan borderFadeDuration = TimeSpan.FromMilliseconds(300);

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
            BorderBrush = new SolidColorBrush(Colors.Transparent);
            Loaded += (s, e) =>
            {
                Task.Delay(1).ContinueWith(
                    t => fadeWindow(true),
                    TaskScheduler.FromCurrentSynchronizationContext());
            };
        }

        Task fadeWindow(bool toShow)
        {
            var mutex = new AutoResetEvent(false);

            var targetOpacity = toShow ? MaxOpacity : 0;
            var animation = new DoubleAnimation(
                targetOpacity, windowFadeDuration);
            animation.Completed += (s, e) => mutex.Set();
            BeginAnimation(OpacityProperty, animation);

            return Task.Run(() => mutex.WaitOne());
        }

        void fadeBorder(bool toActive)
        {
            var targetColor = toActive ? activeBorderColor : Colors.DimGray;
            var animation = new ColorAnimation(targetColor, borderFadeDuration);
            BorderBrush.BeginAnimation(
                SolidColorBrush.ColorProperty, animation);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            fadeBorder(true);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            fadeBorder(false);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Drag the window on left click.
            DragMove();
        }

        void onCloseAnimationComplete(object sender, EventArgs e)
        {
            closeImmediately = true;
            Close();
        }

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (closeImmediately) return;

            e.Cancel = true;
            await fadeWindow(false);
            closeImmediately = true;
            Close();
        }
    }
}
