using System;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Animation
{
    sealed class ReversibleClock
    {
        readonly TimeSpan duration;

        AnimationClock clock;
        TimeSpan prevDuration;

        public ReversibleClock(double msDuration)
        {
            duration = TimeSpan.FromMilliseconds(msDuration);
        }

        public AnimationClock GetAnimationClock(AnimationTimeline timeline)
        {
            // Set timeline duration.
            if (clock == null) timeline.Duration = duration;
            else
            {
                timeline.Duration = duration - prevDuration;
                if (clock.CurrentTime.HasValue)
                {
                    timeline.Duration += clock.CurrentTime.Value;
                }
            }

            // Store duration.
            prevDuration = timeline.Duration.TimeSpan;

            // Create new clock and return it.
            clock = timeline.CreateClock();
            return clock;
        }
    }
}