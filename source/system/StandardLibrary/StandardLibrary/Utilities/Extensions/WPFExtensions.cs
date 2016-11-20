using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace StandardLibrary.Utilities.Extensions
{
    public static class WPFExtensions
    {
        // Get the element with the specified name from the template of this
        // control.
        public static T GetTemplateElement<T>(this Control control, string name) 
            where T : FrameworkElement
        {
            return (T)control.Template.FindName(name, control);
        }

        public static T GetResource<T>(
            this FrameworkElement element, string name)
        {
            return (T)element.FindResource(name);
        }

        // Get the window handle.
        public static IntPtr GetHandle(this Window window)
            => new WindowInteropHelper(window).Handle;
    }
}