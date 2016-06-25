using System.Windows;
using System.Windows.Media;

namespace War
{
    sealed class BattlefieldVisualHost : FrameworkElement
    {
        readonly VisualCollection children;

        public BattlefieldVisualHost()
        {
            children = new VisualCollection(this);
        }

        // Required rendering overrides.
        protected override int VisualChildrenCount => children.Count;
        protected override Visual GetVisualChild(int index) => children[index];

        // Support dragging on this entire control.
        protected override HitTestResult HitTestCore(
            PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}