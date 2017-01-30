using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class TextArea : RichTextBox
    {
        const double TEXT_FADE_DURATION = 200;

        static TextArea()
        {
            // Associate this control with the default theme.
            var type = typeof(TextArea);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        protected Size CharDimensions { get; }

        public TextArea()
        {
            CharDimensions = getCharDimensions();
        }

        Brush createFadingBrush(Brush baseBrush, double duration)
        {
            var brush = baseBrush.Clone();
            brush.BeginAnimation(Brush.OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(duration)
            });
            return brush;
        }

        protected Run Write(string text, Brush foreground)
        {
            var run = new Run(text, CaretPosition)
            {
                Foreground = createFadingBrush(foreground, TEXT_FADE_DURATION)
            };
            CaretPosition = run.ContentEnd;
            return run;
        }

        protected Run WriteLine() => WriteLine(string.Empty, Foreground);

        protected Run WriteLine(string text, Brush foreground) =>
            Write($"{text}\r", foreground);

        protected void ClearAllText() => Document.Blocks.Clear();

        protected TextPointer StartPointer => CaretPosition.DocumentStart;
        protected TextPointer EndPointer =>
            CaretPosition.DocumentEnd
                // Omit default newline that is at the end of the document.
                .GetNextInsertionPosition(LogicalDirection.Backward);

        protected TextPointer LineStartPointer =>
            CaretPosition.GetLineStartPosition(0);

        protected void ClearSelection() =>
            Selection.Select(CaretPosition, CaretPosition);
        
        Size getCharDimensions()
        {
            var formattedText = new FormattedText(
                default(char).ToString(),
                CultureInfo.CurrentUICulture,
                FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Foreground);
            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
