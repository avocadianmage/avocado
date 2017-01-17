using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StandardLibrary.WPF
{
    public sealed class Draggable
    {
        public static void Hook(FrameworkElement element)
            => new Draggable(element);

        readonly FrameworkElement element;

        Draggable(FrameworkElement element)
        {
            this.element = element;
            element.MouseLeftButtonDown += onMouseLeftButtonDown;
            element.MouseLeftButtonUp += onMouseLeftButtonUp;
            element.MouseMove += onMouseMove;
        }

        readonly TranslateTransform transform = new TranslateTransform();
        Point? anchorPoint;

        void onMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => anchorPoint = e.GetPosition(null);

        void onMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => anchorPoint = null;

        void onMouseMove(object sender, MouseEventArgs e)
        {
            if (!anchorPoint.HasValue) return;
            var currentPoint = e.GetPosition(null);
            transform.X += currentPoint.X - anchorPoint.Value.X;
            transform.Y += currentPoint.Y - anchorPoint.Value.Y;
            element.RenderTransform = transform;
            anchorPoint = currentPoint;
        }
    }
}