using AvocadoFramework.Engine;
using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls
{
    public class BaseControl : Control
    {
        public BaseControl()
        {
            Loaded += (sender, e) => OnLoad(e);
            Unloaded += (sender, e) => OnUnload(e);
        }

        protected virtual void OnLoad(RoutedEventArgs e)
        {
            // Base implementation does nothing.
        }

        protected virtual void OnUnload(RoutedEventArgs e)
        {
            // Base implementation does nothing.
        }

        protected void SetWindowTitle(string title)
            => parentWindow.Title = title;

        protected void CloseWindow() => parentWindow.Close();

        GlassPane parentWindow => Window.GetWindow(this) as GlassPane;
    }
}
