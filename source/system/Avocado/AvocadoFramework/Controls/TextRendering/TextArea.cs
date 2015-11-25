using AvocadoFramework.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class TextArea : BaseControl
    {
        static TextArea()
        {
            // Associate this window with the default theme.
            var frameType = typeof(TextArea);
            DefaultStyleKeyProperty.OverrideMetadata(
                frameType,
                new FrameworkPropertyMetadata(frameType));
        }
        
        protected RichTextBox TextBase => textBase;
        RichTextBox textBase;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Cache any template child controls.
            textBase = this.GetTemplateElement<RichTextBox>("textBase");
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // Disallow mouse interaction with text. Instead, continue to 
            // support dragging the window.
            e.Handled = true;
            Window.GetWindow(this).DragMove();
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

        protected void WriteLine() 
            => WriteLine(string.Empty, Brushes.Transparent);

        protected void WriteLine(string text, Brush foreground)
            => Write($"{text}\r", foreground);

        protected void Clear() => TextBase.Document.Blocks.Clear();

        protected void SetDefaultForeground()
        {
            TextBase.Selection.ApplyPropertyValue(
                TextElement.ForegroundProperty,
                Foreground);
        }

        protected void MoveCaretToDocumentEnd()
            => TextBase.CaretPosition = TextBase.CaretPosition.DocumentEnd;

        protected void MoveCaret(int offset, bool select)
        {
            var before = TextBase.CaretPosition;
            var after = before.GetPositionAtOffset(offset);

            // If requested, select the contents in between.
            if (select) TextBase.Selection.Select(before, after);
            else TextBase.CaretPosition = after;
        }

        protected int CaretX
        {
            get
            {
                var current = TextBase.CaretPosition;
                var paragraph = current.Paragraph;
                var range = new TextRange(paragraph.ContentStart, current);
                return range.Text.Length;
            }
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
