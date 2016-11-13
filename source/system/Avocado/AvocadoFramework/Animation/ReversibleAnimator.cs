using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Animation
{
    sealed class ReversibleAnimator<T>
    {
        public event EventHandler StartReached;
        public event EventHandler EndReached;

        readonly IEnumerable<dynamic> targetObjs;
        readonly DependencyProperty property;
        readonly T start;
        readonly T end;

        readonly ReversibleClock reversibleClock;

        public ReversibleAnimator(
            DependencyProperty property, 
            T start, 
            T end, 
            double msDuration,
            params UIElement[] targetObjs)
            : this(property, start, end, msDuration)
        {
            this.targetObjs = targetObjs;
        }

        public ReversibleAnimator(
            DependencyProperty property,
            T start,
            T end,
            double msDuration,
            params Animatable[] targetObjs)
            : this(property, start, end, msDuration)
        {
            this.targetObjs = targetObjs;
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
            foreach (var obj in targetObjs)
                obj.ApplyAnimationClock(property, clock);
        }

        static AnimationTimeline createTimelineInstance()
        {
            var baseType = typeof(AnimationTimeline);
            var assembly = baseType.Assembly.FullName;
            var typeName = $"{baseType.Namespace}.{typeof(T).Name}Animation";
            var handle = Activator.CreateInstance(assembly, typeName);
            return handle.Unwrap() as AnimationTimeline;
        }
    }
}