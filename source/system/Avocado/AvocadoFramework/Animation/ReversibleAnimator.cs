using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Animation
{
    sealed class ReversibleAnimator<T>
    {
        public event EventHandler StartReached;
        public event EventHandler EndReached;

        readonly dynamic targetObj;
        readonly DependencyProperty property;
        readonly T start;
        readonly T end;

        readonly ReversibleClock reversibleClock;

        public ReversibleAnimator(
            UIElement targetObj,
            DependencyProperty property, 
            T start, 
            T end, 
            double msDuration)
            : this(property, start, end, msDuration)
        {
            this.targetObj = targetObj;
        }

        public ReversibleAnimator(
            Animatable targetObj,
            DependencyProperty property,
            T start,
            T end,
            double msDuration)
            : this(property, start, end, msDuration)
        {
            this.targetObj = targetObj;
        }

        ReversibleAnimator(
            DependencyProperty property,
            T start,
            T end,
            double msDuration)
        {
            this.property = property;
            this.start = start;
            this.end = end;
            reversibleClock = new ReversibleClock(msDuration);
        }

        public void Animate(bool forward)
        {
            // Create new animation timeline.
            dynamic timeline = createTimelineInstance();
            timeline.To = forward ? end : start;
            
            // Create new animation clock from the timeline.
            AnimationClock clock 
                = reversibleClock.GetAnimationClock(timeline);
            clock.Completed += (sender, e) =>
            {
                var handler = forward ? EndReached : StartReached;
                handler?.Invoke(sender, e);
            };

            // Run animation.
            targetObj.ApplyAnimationClock(property, clock);
        }

        static AnimationTimeline createTimelineInstance()
        {
            var baseType = typeof(AnimationTimeline);
            var assembly = baseType.Assembly.FullName;
            var typeName = string.Format(
                "{0}.{1}Animation",
                baseType.Namespace,
                typeof(T).Name);
            var handle = Activator.CreateInstance(assembly, typeName);
            return handle.Unwrap() as AnimationTimeline;
        }
    }
}