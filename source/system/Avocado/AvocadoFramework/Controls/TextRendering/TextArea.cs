using System;
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

        protected RichTextBox TextBase 
            => this.GetTemplateElement<RichTextBox>("textBase");

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // Disallow mouse interaction with text. Instead, continue to 
            // support dragging the window.
            e.Handled = true;
            Window.GetWindow(this).DragMove();
        }

        TextRange write(string text)
        {
            var pos = TextBase.CaretPosition;
            var range = new TextRange(pos, pos) { Text = text };
            TextBase.CaretPosition = range.End;
            return range;
        }

        protected void Write(string text, Brush foreground)
        {
            write(text).ApplyPropertyValue(
                TextElement.ForegroundProperty, 
                foreground);
        }

        protected void WriteLine() => write(Environment.NewLine);

        protected void WriteLine(string text, Brush foreground)
        {
            Write(text, foreground);
            WriteLine();
        }

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
                var start = current.Paragraph.ContentStart;
                return new TextRange(start, current).Text.Length;
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
