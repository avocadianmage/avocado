using System;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Controls.TextRendering
{
    static class TextAnimation
    {
        public static Brush GetFadingBrush(Brush baseBrush, double duration)
            => GetFadingBrush(baseBrush, duration, false);

        public static Brush GetFadingBrush(
            Brush baseBrush, double duration, bool oscillating)
        {
            var brush = baseBrush.Clone();
            var animation = createAnimation(duration, oscillating);

            // Start animation.
            brush.BeginAnimation(Brush.OpacityProperty, animation);

            return brush;
        }

        static DoubleAnimation createAnimation(
            double duration, bool oscillating)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
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