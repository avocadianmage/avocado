using AvocadoFramework.Animation;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.Processes;

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
            base.OnContentRendered(e);

            // Set initial focus.
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // Show the window.
            initializeWindowFading();
            windowFadeAnimator.Animate(true);
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
                0,
                1,
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
            // Use different active border color based on whether the program
            // has elevated permissions.
            var activeBorderColor = EnvUtils.IsAdmin
                ? Config.ActiveBorderColorElevated : Config.ActiveBorderColor;

            borderFadeAnimator = new ReversibleAnimator<Color>(
                BorderBrush,
                SolidColorBrush.ColorProperty,
                Config.InactiveBorderColor,
                activeBorderColor,
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
