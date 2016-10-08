using AvocadoFramework.Animation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class TextArea : Control
    {
        static TextArea()
        {
            // Associate this control with the default theme.
            var type = typeof(TextArea);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        public static Brush CreateTextFadeBrush(Brush baseBrush)
        {
            return new BrushAnimation()
                .GetFadingBrush(baseBrush, Config.TextFadeDuration);
        }

        protected RichTextBox TextBase { get; private set; }
        protected Size CharDimensions { get; private set; }

        protected TextPointer CaretPointer
        {
            get { return TextBase.CaretPosition; }
            set { TextBase.CaretPosition = value; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Cache any template child controls.
            TextBase = this.GetTemplateElement<RichTextBox>("textBase");
            CharDimensions = getCharDimensions();
        }

        protected void Write(string text, Brush foreground)
        {
            CaretPointer = new Run(text, CaretPointer)
            {
                Foreground = CreateTextFadeBrush(foreground)
            }
            .ContentEnd;
        }

        protected void WriteLine() => WriteLine(string.Empty, Foreground);

        protected void WriteLine(string text, Brush foreground)
        {
            text = $"{text.TrimEnd()}{Environment.NewLine}";
            Write(text, foreground);
        }

        protected TextPointer StartPointer => CaretPointer.DocumentStart;
        protected TextPointer EndPointer
            => CaretPointer.DocumentEnd
                // Omit default newline that is at the end of the document.
                .GetNextInsertionPosition(LogicalDirection.Backward);

        protected TextPointer LineStartPointer 
            => CaretPointer.GetLineStartPosition(0);

        protected void MoveCaret(TextPointer pointer, bool select)
        {
            if (select) TextBase.Selection.Select(CaretPointer, pointer);
            else CaretPointer = pointer;
        }

        protected void ClearSelection()
            => TextBase.Selection.Select(CaretPointer, CaretPointer);
        
        Size getCharDimensions()
        {
            var formattedText = new FormattedText(
                default(char).ToString(),
                CultureInfo.CurrentUICulture,
                TextBase.FlowDirection,
                new Typeface(
                    TextBase.FontFamily,
                    TextBase.FontStyle,
                    TextBase.FontWeight,
                    TextBase.FontStretch),
                TextBase.FontSize,
                TextBase.Foreground);
            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
