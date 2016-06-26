using System;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Animation
{
    sealed class BrushAnimation
    {
        public Brush GetFadingBrush(Brush baseBrush, double duration)
            => GetFadingBrush(baseBrush, duration, false);

        public Brush GetFadingBrush(
            Brush baseBrush, double duration, bool oscillating)
        {
            var brush = getZeroOpacityBrush(baseBrush);
            var animation = getAnimation(duration, oscillating);

            // Start animation.
            brush.BeginAnimation(Brush.OpacityProperty, animation);

            return brush;
        }

        static Brush getZeroOpacityBrush(Brush baseBrush)
        {
            var brush = baseBrush.Clone();
            brush.Opacity = 0;
            return brush;
        }

        static DoubleAnimation getAnimation(double duration, bool oscillating)
        {
            var animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(duration)
            };

            // If the flag is true, have the animation cycle indefinitely.
            if (oscillating)
            {
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
            }

            return animation;
        }
    }
}