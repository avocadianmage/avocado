using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public static void BindCommand(
            this UIElement element, 
            ICommand command,
            Action action)
        {
            element.CommandBindings.Add(
                new CommandBinding(command, (s, e) => action()));
        }

        public static void MoveNextFocus(this FrameworkElement element)
            => element.MoveFocus(new TraversalRequest(
                FocusNavigationDirection.Next));

        // Get the window handle.
        public static IntPtr GetHandle(this Window window)
            => new WindowInteropHelper(window).Handle;
    }
}