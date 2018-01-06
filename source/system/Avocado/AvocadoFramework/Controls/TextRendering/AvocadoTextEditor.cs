using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using StandardLibrary.WPF;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class AvocadoTextEditor : TextEditor
    {
        static AvocadoTextEditor()
        {
            // Associate this control with the default theme.
            var type = typeof(AvocadoTextEditor);
            DefaultStyleKeyProperty.OverrideMetadata(
                type, new FrameworkPropertyMetadata(type));
        }

        readonly StaticColorizer staticColorizer = new StaticColorizer();
        bool shouldAutoScroll;
        Border visualCaret;

        public AvocadoTextEditor()
        {
            TextArea.Caret.CaretBrush = Brushes.Transparent;

            Loaded += onLoaded;
            Document.Changing += onDocumentChanging;
            Document.Changed += onDocumentChanged;

            TextArea.TextView.LineTransformers.Add(staticColorizer);
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            visualCaret.Height 
                = TextArea.TextView.GetVisualLine(TextArea.Caret.Line).Height;
        }

        void updateVisualCaretPosition()
        {
            var caretVisualPosition = TextArea.TextView.GetVisualPosition(
                TextArea.Caret.Position, VisualYPosition.LineTop)
                - TextArea.TextView.ScrollOffset;
            Canvas.SetLeft(visualCaret, caretVisualPosition.X);
            Canvas.SetTop(visualCaret, caretVisualPosition.Y);
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            initializeVisualCaret();
        }

        void initializeVisualCaret()
        {
            visualCaret = this.GetTemplateElement<Border>("Caret");

            // Add positioning events.
            TextArea.Caret.PositionChanged +=
                (s, e) => updateVisualCaretPosition();
            AddHandler(
                ScrollViewer.ScrollChangedEvent,
                new ScrollChangedEventHandler(
                    (s, e) => updateVisualCaretPosition()));

            // Window events (Window.GetWindow returns null in design mode).
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var window = Window.GetWindow(this);
                window.Activated +=
                    (s, e) => visualCaret.Visibility = Visibility.Visible;
                window.Deactivated +=
                    (s, e) => visualCaret.Visibility = Visibility.Collapsed;
            }
        }

        protected void SetSyntaxHighlighting(string manifestResourceStreamPath)
        {
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

        protected void Append(string text, Brush foreground)
        {
            Document.BeginUpdate();

            var start = Document.TextLength;
            var line = Document.LineCount;
            Document.Insert(start, text);

            // Add each line to the static colorizer with the specified 
            // foreground.
            var fadingBrush = createFadingBrush(
                foreground, TextConfig.TextFadeDuration);
            for (; line <= Document.LineCount; line++)
            {
                var lineObject = Document.GetLineByNumber(line);
                staticColorizer.AddColoredLinePart(
                    line, 
                    Math.Max(lineObject.Offset, start), 
                    Math.Min(lineObject.EndOffset, Document.TextLength),
                    fadingBrush);
            }

            Document.EndUpdate();
        }

        protected void AppendLine()
        {
            Document.Insert(Document.TextLength, Environment.NewLine);
        }

        protected void AppendLine(string text, Brush foreground)
        {
            Append(text + Environment.NewLine, foreground);
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
