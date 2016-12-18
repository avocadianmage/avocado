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

        protected void Write(string text, Brush foreground) =>
            CaretPosition = new Run(text, CaretPosition)
            {
                Foreground = createFadingBrush(foreground, TEXT_FADE_DURATION)
            }
            .ContentEnd;

        protected void WriteLine() => WriteLine(string.Empty, Foreground);

        protected void WriteLine(string text, Brush foreground) =>
            Write($"{text.TrimEnd()}\r", foreground);

        protected TextPointer StartPointer => CaretPosition.DocumentStart;
        protected TextPointer EndPointer =>
            CaretPosition.DocumentEnd
                // Omit default newline that is at the end of the document.
                .GetNextInsertionPosition(LogicalDirection.Backward);

        protected TextPointer LineStartPointer =>
            CaretPosition.GetLineStartPosition(0);

        protected void MoveCaret(TextPointer pointer, bool select)
        {
            if (select) Selection.Select(CaretPosition, pointer);
            else CaretPosition = pointer;
        }

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
