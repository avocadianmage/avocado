using StandardLibrary.Processes;
using StandardLibrary.Utilities.Extensions;
using System;
using System.ComponentModel;
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

        // Startup code.
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hookMouseEvents();
        }

        void hookMouseEvents()
        {
            var contentArea 
                = this.GetTemplateElement<ContentPresenter>("ContentArea");
            contentArea.PreviewMouseDown += onContentAreaPreviewMouseDown;
            contentArea.PreviewMouseUp += onContentAreaPreviewMouseUp;
        }

        void onContentAreaPreviewMouseDown(
            object sender, MouseButtonEventArgs e)
        {
            // Prevent the mouse from interacting with other controls.
            e.Handled = true;

            if (e.LeftButton != MouseButtonState.Pressed) return;

            // Double-click to toggle maximize.
            if (e.ClickCount == 2 && ResizeMode != ResizeMode.NoResize)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal : WindowState.Maximized;
            }
            
            // Otherwise, drag the window around.
            else DragMove();
        }

        void onContentAreaPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Prevent the mouse from interacting with other controls.
            e.Handled = true;
        }

        void applyCloseAnimation()
        {
            var storyboard = paneUI.GetResource<Storyboard>("WindowFadeOut");
            storyboard.Completed += onCloseAnimationComplete;
            storyboard.Begin();
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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            paneUI.GetResource<Storyboard>("OutlineFadeIn").Begin();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            paneUI.GetResource<Storyboard>("OutlineFadeOut").Begin();
        }
    }
}
