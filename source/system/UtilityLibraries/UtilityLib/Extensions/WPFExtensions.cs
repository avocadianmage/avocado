using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace UtilityLib.Extensions
{
    public static class WPFExtensions
    {
        // Get the element with the specified name from the template of this
        // control.
        public static T GetTemplateElement<T>(
            this Control control, string name) where T : FrameworkElement
        {
            return (T)control.Template.FindName(name, control);
        }

        public static void RegisterPropertyOnChange(
            this FrameworkElement element,
            DependencyProperty property,
            EventHandler handler)
        {
            var desc = DependencyPropertyDescriptor.FromProperty(
                property,
                element.GetType());
            desc.AddValueChanged(element, handler);
        }

        public static IntPtr GetHandle(this Window window)
            => new WindowInteropHelper(window).Handle;
    }
}