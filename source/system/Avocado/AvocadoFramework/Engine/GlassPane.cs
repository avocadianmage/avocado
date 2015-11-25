using AvocadoFramework.Animation;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AvocadoFramework.Engine
{
    public abstract class GlassPane : Window
    {
        ReversibleAnimator<double> windowFadeAnimator;
        ReversibleAnimator<Color> borderFadeAnimator;
        bool closeImmediately = false;
        
        static GlassPane()
        {
            // Associate this window with the default theme.
            var frameType = typeof(GlassPane);
            DefaultStyleKeyProperty.OverrideMetadata(
                frameType,
                new FrameworkPropertyMetadata(frameType));
        }

        // Initialization and startup code.
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Show the window.
            initializeWindowFading();
            windowFadeAnimator.Animate(true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Allow the window to be dragged via the left-mouse button.
            DragMove();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // Transition to active border color.
            fadeBorder(true);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            
            // Transition to inactive border color.
            fadeBorder(false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // If the window can close immediately, have it proceed 
            // unhindered.
            if (closeImmediately) return;

            // Otherwise, cancel the close operation.
            e.Cancel = true;

            // Start the fade out animation.
            windowFadeAnimator.Animate(false);
        }

        void initializeWindowFading()
        {
            windowFadeAnimator = new ReversibleAnimator<double>(
                this,
                OpacityProperty,
                Config.MinOpacity,
                Config.MaxOpacity,
                Config.WindowFadeDuration);
            windowFadeAnimator.StartReached += onWindowFadeOut;
            windowFadeAnimator.EndReached += onWindowFadeIn;
        }

        void onWindowFadeOut(object sender, EventArgs e)
        {
            // Actually close the window.
            closeImmediately = true;
            Close();
        }

        void onWindowFadeIn(object sender, EventArgs e)
        {
            initializeBorderFading();

            // Transition to active border color.
            fadeBorder(true);
        }

        void initializeBorderFading()
        {
            borderFadeAnimator = new ReversibleAnimator<Color>(
                BorderBrush,
                SolidColorBrush.ColorProperty,
                Config.InactiveBorderColor,
                Config.ActiveBorderColor,
                Config.BorderFadeDuration);
        }

        void fadeBorder(bool fadeIn)
        {
            // Quit if the animator is not yet initialized.
            if (borderFadeAnimator == null) return;

            // Quit if the fade operation does not align with the active
            // property of the window.
            if (fadeIn != IsActive) return;

            borderFadeAnimator.Animate(fadeIn);
        }
    }
}
