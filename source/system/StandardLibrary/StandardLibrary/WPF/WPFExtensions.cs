using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StandardLibrary.WPF
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
            this UIElement element, ICommand command, Action action)
        {
            element.CommandBindings.Add(
                new CommandBinding(command, (s, e) => action()));
        }

        public static void MoveNextFocus(this FrameworkElement element)
            => element.MoveFocus(new TraversalRequest(
                FocusNavigationDirection.Next));

        public static void SubscribeToPropertyChange(
            this DependencyObject target, 
            DependencyProperty property, 
            EventHandler handler)
        {
            DependencyPropertyDescriptor
                .FromProperty(property, target.GetType())
                .AddValueChanged(target, handler);
        }
    }
}