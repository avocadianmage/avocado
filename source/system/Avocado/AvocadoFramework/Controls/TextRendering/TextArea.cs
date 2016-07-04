using AvocadoFramework.Animation;
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Cache any template child controls.
            textBase = this.GetTemplateElement<RichTextBox>("textBase");
        }

        protected void Write(string text, Brush foreground)
        {
            // Insert the text at the caret position.
            var pos = textBase.CaretPosition;
            var range = new TextRange(pos, pos) { Text = text };

            // Apply color and animation to the newly inserted text.
            range.ApplyPropertyValue(
                TextElement.ForegroundProperty,
                createFadeInBrush(foreground));

            // Move the caret to the end of the inserted text.
            textBase.CaretPosition = range.End;
        }

        static Brush createFadeInBrush(Brush baseBrush)
        {
            return new BrushAnimation().GetFadingBrush(
                baseBrush,
                Config.TextFadeDuration);
        }

        protected void WriteLine() => WriteLine(string.Empty, Foreground);

        protected void WriteLine(string text, Brush foreground)
            => Write($"{text.TrimEnd()}\r", foreground);

        protected void MoveCaretToDocumentEnd()
            => textBase.CaretPosition = paragraph.ContentEnd;

        protected void MoveCaret(int offset, bool select)
        {
            var before = textBase.CaretPosition;
            var after = before.GetPositionAtOffset(offset);

            // If requested, select the contents in between.
            if (select) textBase.Selection.Select(before, after);
            else textBase.CaretPosition = after;
        }

        protected void ClearSelection()
        {
            textBase.Selection.Select(
                textBase.CaretPosition, textBase.CaretPosition);
        }

        protected int GetX(TextPointer pointer)
        {
            // Use length of text, not symbols.
            var range = new TextRange(pointer.Paragraph.ContentStart, pointer);
            return range.Text.Length;
        }
        
        protected string FullText
            => new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;

        Paragraph paragraph
            => textBase.CaretPosition.Paragraph 
            ?? textBase.CaretPosition
                .GetNextInsertionPosition(LogicalDirection.Backward)
                .Paragraph;
        
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
