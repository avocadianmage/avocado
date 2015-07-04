using AvocadoFramework.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvocadoFramework.Controls.Text.Input
{
    sealed class CaretUI : FrameworkElement
    {
        readonly Rect drawingRect = new Rect(TextProperties.CharDimensions);
        TextLine parent;

        public CaretUI()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        public void SetPosition(TextLine newParent, int gridX)
        {
            // Set the parent TextLine of the caret.
            updateParent(newParent);

            // Update the position of the caret in the TextLine.
            updatePosition(gridX);
            
            // Scroll the caret into view.
            BringIntoView(getBounds());
        }

        Rect getBounds()
        {
            var rect = drawingRect;
            rect.Inflate(Config.CaretThickness, Config.CaretThickness);
            return rect;
        }

        void updateParent(TextLine newParent)
        {
            if (parent == newParent) return;

            if (parent != null) parent.Children.Remove(this);
            newParent.Children.Add(this);
            parent = newParent;
        }

        void updatePosition(int gridX)
        {
            var position = parent.GetSegmentPosition(gridX);
            Canvas.SetLeft(this, position.X);
            Canvas.SetTop(this, position.Y);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            renderCaret(drawingContext);
        }

        void renderCaret(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(null, createCaretPen(), drawingRect);
        }

        static Pen createCaretPen()
        {
            // Create gradient brush.
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Config.CaretColor, 0.125),
                    new GradientStop(Colors.Transparent, 0.125),
                    new GradientStop(Colors.Transparent, 0.8),
                    new GradientStop(Config.CaretColor, 0.8),
                },
            };

            // Create animated brush.
            var animation = new BrushAnimation();
            var animatedBrush = animation.GetFadingBrush(
                brush,
                Config.CaretFadeDuration,
                true);

            // Create and return pen object.
            return new Pen(animatedBrush, Config.CaretThickness);
        }
    }
}