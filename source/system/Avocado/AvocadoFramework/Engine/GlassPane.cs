using StandardLibrary.Processes;
using StandardLibrary.WPF;
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

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // Drag the window around when the left mouse button is pressed.
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
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
    }
}
