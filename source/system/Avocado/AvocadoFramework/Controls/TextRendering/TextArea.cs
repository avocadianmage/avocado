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

        protected RichTextBox TextBase => textBase;
        RichTextBox textBase;

        protected TextPointer CaretPointer
        {
            get { return textBase.CaretPosition; }
            set { textBase.CaretPosition = value; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Cache any template child controls.
            textBase = this.GetTemplateElement<RichTextBox>("textBase");
        }

        protected void Write(string text, Brush foreground)
        {
            CaretPointer = new Run(text, CaretPointer)
            {
                Foreground = foreground
            }
            .ContentEnd;
        }

        protected void WriteLine() => WriteLine(string.Empty, Foreground);

        protected void WriteLine(string text, Brush foreground)
            => Write($"{text.TrimEnd()}\r", foreground);

        protected TextPointer StartPointer => paragraph.ContentStart;
        protected TextPointer EndPointer => paragraph.ContentEnd;
        protected TextPointer LineStartPointer 
            => CaretPointer.GetLineStartPosition(0);

        Paragraph paragraph
            => CaretPointer.Paragraph
            ?? CaretPointer
                .GetNextInsertionPosition(LogicalDirection.Backward)
                .Paragraph;

        protected void MoveCaret(TextPointer pointer, bool select)
        {
            if (select) textBase.Selection.Select(CaretPointer, pointer);
            else CaretPointer = pointer;
        }

        protected void ClearSelection()
            => textBase.Selection.Select(CaretPointer, CaretPointer);
        
        protected Size GetCharDimensions()
        {
            var formattedText = new FormattedText(
                default(char).ToString(),
                CultureInfo.CurrentUICulture,
                textBase.FlowDirection,
                new Typeface(
                    textBase.FontFamily,
                    textBase.FontStyle,
                    textBase.FontWeight,
                    textBase.FontStretch),
                textBase.FontSize,
                textBase.Foreground);
            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
