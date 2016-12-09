using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

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

        public static void InvokeOnUIThread(
            this DispatcherObject dispatcherObject, Action action)
        {
            dispatcherObject.InvokeOnUIThread(
                action, DispatcherPriority.Normal);
        }

        public static void InvokeOnUIThread(
            this DispatcherObject dispatcherObject, 
            Action action, 
            DispatcherPriority priority)
        {
            dispatcherObject.Dispatcher.BeginInvoke(action, priority);
        }

        public static void MoveNextFocus(this FrameworkElement element)
            => element.MoveFocus(new TraversalRequest(
                FocusNavigationDirection.Next));

        // Get the window handle.
        public static IntPtr GetHandle(this Window window)
            => new WindowInteropHelper(window).Handle;
    }
}