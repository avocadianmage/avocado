using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class AvocadoTextEditor : TextEditor
    {
        readonly StaticColorizer staticColorizer = new StaticColorizer();
        bool shouldAutoScroll;

        public AvocadoTextEditor()
        {
            Document.Changing += onDocumentChanging;
            Document.Changed += onDocumentChanged;

            TextArea.TextView.LineTransformers.Add(staticColorizer);
        }
        
        void onDocumentChanged(object sender, DocumentChangeEventArgs e)
        {
            if (shouldAutoScroll) ScrollToEnd();
        }

        void onDocumentChanging(object sender, DocumentChangeEventArgs e)
        {
            shouldAutoScroll 
                = isScrolledToEnd && e.Offset == Document.TextLength;
        }

        bool isScrolledToEnd => VerticalOffset + ViewportHeight >= ExtentHeight;

        protected void SetSyntaxHighlighting(string manifestResourceStreamPath)
        {
            // Does not work in design mode.
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            using (var stream = Assembly.GetEntryAssembly()
                .GetManifestResourceStream(manifestResourceStreamPath))
            using (var reader = new XmlTextReader(stream))
            {
                SyntaxHighlighting = HighlightingLoader.Load(
                    reader, HighlightingManager.Instance);
            }
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

        protected void Write(string text, Brush foreground)
        {
            var start = CaretOffset;
            var line = TextArea.Caret.Line;
            Document.Insert(start, text);

            // Add each line to the static colorizer with the specified 
            // foreground.
            var fadingBrush = createFadingBrush(
                foreground, Config.TextFadeDuration);
            for (; line <= TextArea.Caret.Line; line++)
            {
                var lineObject = Document.GetLineByNumber(line);
                staticColorizer.AddColoredLinePart(
                    line, 
                    Math.Max(lineObject.Offset, start), 
                    Math.Min(lineObject.EndOffset, CaretOffset),
                    fadingBrush);
            }
        }

        protected void WriteLine()
        {
            Document.Insert(CaretOffset, Environment.NewLine);
        }

        protected void WriteLine(string text, Brush foreground)
        {
            Write(text, foreground);
            WriteLine();
        }

        protected new void Select(
            int targetOffsetNoCursor, int targetOffsetCursor)
        {
            base.Select(
                Math.Min(targetOffsetNoCursor, targetOffsetCursor), 
                Math.Abs(targetOffsetNoCursor - targetOffsetCursor));
            CaretOffset = targetOffsetCursor;
        }

        protected new int SelectionStart
        {
            get
            {

                return base.SelectionStart == CaretOffset
                    ? base.SelectionStart + SelectionLength
                    : base.SelectionStart;
            }
            set { throw new NotImplementedException(); }
        }

        protected new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { throw new NotImplementedException(); }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Esc: clear the currently selected text.
                case Key.Escape:
                    TextArea.ClearSelection();
                    break;
            }
            
            base.OnPreviewKeyDown(e);
        }
    }
}
