using AvocadoFramework.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class TextArea : BaseControl
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
            var pos = TextBase.CaretPosition;
            var range = new TextRange(pos, pos) { Text = text };

            // Apply color and animation to the newly inserted text.
            range.ApplyPropertyValue(
                TextElement.ForegroundProperty,
                createFadeInBrush(foreground));

            // Move the caret to the end of the inserted text.
            TextBase.CaretPosition = range.End;
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
        {
            TextBase.CaretPosition
                = TextBase.CaretPosition.Paragraph.ContentEnd;
        }

        protected void MoveCaret(int offset, bool select)
        {
            var before = TextBase.CaretPosition;
            var after = before.GetPositionAtOffset(offset);

            // If requested, select the contents in between.
            if (select) TextBase.Selection.Select(before, after);
            else TextBase.CaretPosition = after;
        }

        protected void ClearSelection()
        {
            TextBase.Selection.Select(
                TextBase.CaretPosition, TextBase.CaretPosition);
        }

        protected int GetX(TextPointer pointer)
        {
            // Use length of text, not symbols.
            var range = new TextRange(pointer.Paragraph.ContentStart, pointer);
            return range.Text.Length;
        }

        protected string CurrentLineString
        {
            get
            {
                var paragraph = TextBase.CaretPosition.Paragraph;
                var start = paragraph.ContentStart;
                var end = paragraph.ContentEnd;
                return new TextRange(start, end).Text;
            }
        }
    }
}
