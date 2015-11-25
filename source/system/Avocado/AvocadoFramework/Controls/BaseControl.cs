using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoFramework.Controls
{
    public class BaseControl : Control
    {
        public BaseControl()
        {
            Loaded += (sender, e) => OnLoad(e);
            Unloaded += (sender, e) => OnUnload(e);
        }

        #region Window APIs

        Window parentWindow => Window.GetWindow(this);

        protected void SetWindowTitle(string title)
            => parentWindow.Title = title;

        protected void CloseWindow() => parentWindow.Close();

        #endregion

        #region Overridable events

        protected virtual void OnLoad(RoutedEventArgs e)
        {
            // Base implementation does nothing.
        }

        protected virtual void OnUnload(RoutedEventArgs e)
        {
            // Base implementation does nothing.
        }

        #endregion

        #region Suppress native control mouse input

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            e.Handled = true;

            // Allow left-click to drag window.
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(this).DragMove();
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            e.Handled = true;
        }

        #endregion
    }
}
