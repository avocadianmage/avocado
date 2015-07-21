using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UtilityLib.Legacy;

namespace AvocadoFramework.Controls.Text
{
    sealed class TextLine : Canvas
    {
        public event EventHandler RenderFinished;

        public ObservableList<Segment> SegmentData
        {
            get { return segmentData; }
        }

        readonly ObservableList<Segment> segmentData;

        FormattedText textToRender;

        // True if we have permission to perform our expensive rendering
        // operations.
        bool renderAllowed = false;

        public TextLine(IEnumerable<Segment> segments)
        {
            // Initialize segment data list structure.
            segmentData = new ObservableList<Segment>(segments);
            
            // Perform initial render upon load.
            Loaded += onLoaded;
        }

        void onLoaded(object sender, RoutedEventArgs e)
        {
            // Perform the initial render.
            InvalidateVisual();
            
            // Subscribe to segment data changes for further rendering 
            // directives.
            segmentData.ListChanged += (sender2, e2) => InvalidateVisual();
        }

        public new void InvalidateVisual()
        {
            // Update our text to render.
            textToRender = createFormattedText(GetLineText());
            applyStylings(textToRender);

            // Update our element height, if we need to.
            setHeight(textToRender);

            // Allow our rendering operation to proceed.
            renderAllowed = true;

            // Perform the actual rendering.
            base.InvalidateVisual();

            // Fire RenderFinished event.
            RenderFinished?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (!renderAllowed) return;

            renderAllowed = false;
            renderText(drawingContext);
        }

        void renderText(DrawingContext drawingContext)
        {
            // Do not render anything if the text is only white space.
            if (string.IsNullOrWhiteSpace(textToRender.Text)) return;

            drawingContext.DrawText(textToRender, new Point());
        }

        void setHeight(FormattedText text)
        {
            var height = Math.Max(
                text.Height, 
                TextProperties.CharDimensions.Height);
            if (Height != height) Height = height;
        }

        FormattedText createFormattedText(string text)
        {
            var frmText = TextProperties.CreateFormattedText(text);
            frmText.MaxTextWidth = ActualWidth;
            frmText.TextAlignment = container.TextAlign;
            return frmText;
        }

        void applyStylings(FormattedText formattedText)
        {
            var textIndex = 0;
            foreach (var segment in segmentData)
            {
                var length = segment.Text.Length;
                formattedText.SetForegroundBrush(
                    segment.Foreground,
                    textIndex,
                    length);

                textIndex += length;
            }
        }

        public string GetLineText()
        {
            return getLineText(segmentData.Count);
        }

        string getLineText(int segmentCount)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < segmentCount; i++)
            {
                builder.Append(segmentData[i].Text);
            }
            return builder.ToString();
        }

        public Point GetSegmentPosition(int segmentCount)
        {
            var basePoint = new Point(
                Config.CaretThickness,
                Config.CaretThickness);

            if (segmentCount == 0) return basePoint;

            // Return the top-right position of the character right before the 
            // cursor.
            var str = getLineText(segmentCount);
            var fmtText = textToRender ?? createFormattedText(GetLineText());
            var geometry = fmtText.BuildHighlightGeometry(
                basePoint, str.Length - 1, 1);
            return geometry.Bounds.TopRight;
        }

        TextContainer container
        {
            get { return Parent as TextContainer; }
        }
    }
}