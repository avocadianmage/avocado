using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        public static int GetOffsetInRange(
            this TextPointer pointer, TextRange range)
        {
            if (range.Start.GetOffsetToPosition(pointer) >= 0
                && range.End.GetOffsetToPosition(pointer) <= 0)
            {
                return new TextRange(range.Start, pointer).Text.Length;
            }
            return -1;
        }

        public static TextPointer GetPointerFromCharOffset(
            this TextPointer pointer, int offset)
        {
            var count = 0;
            while (pointer != null)
            {
                var nextPointerContext = pointer.GetPointerContext(
                    LogicalDirection.Forward);
                if (nextPointerContext == TextPointerContext.Text)
                {
                    var runLength = pointer.GetTextRunLength(
                        LogicalDirection.Forward);
                    if (runLength > 0 && runLength + count < offset)
                    {
                        count += runLength;
                        pointer = pointer.GetPositionAtOffset(runLength);

                        if (count <= offset) continue;
                    }
                    else count++;
                }
                else if (nextPointerContext == TextPointerContext.ElementEnd)
                {
                    var element = pointer.GetAdjacentElement(
                        LogicalDirection.Forward);
                    if (element is LineBreak || element is Paragraph)
                        count += Environment.NewLine.Length;
                }

                if (count > offset) break;

                pointer = pointer.GetPositionAtOffset(
                    1, LogicalDirection.Forward);
            }
            return pointer;
        }
    }
}