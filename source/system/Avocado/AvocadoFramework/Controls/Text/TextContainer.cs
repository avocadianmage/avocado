using AvocadoFramework.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvocadoFramework.Controls.Text
{
    public class TextContainer : StackPanel
    {
        public static readonly DependencyProperty TextAlignProperty
            = DependencyProperty.Register(
                "TextAlign",
                typeof(TextAlignment),
                typeof(SimpleLabel),
                new FrameworkPropertyMetadata(TextAlignment.Left));

        public TextAlignment TextAlign
        {
            get { return (TextAlignment)GetValue(TextAlignProperty); }
            set { SetValue(TextAlignProperty, value); }
        }

        internal event EventHandler<CaretMovedEventArgs> CaretMoved;

        readonly ObservableCollection<TextLine> lines 
            = new ObservableCollection<TextLine>();

        CaretMarker caret;

        public TextContainer()
        {
            initializeCaret();
            setProperties();
            prepareLineData();
        }

        void initializeCaret()
        {
            caret = new CaretMarker(lines);
            caret.Translated += (sender, e) => updateCaretDisplay();
        }

        void setProperties()
        {
            Margin = new Thickness(
                Config.CaretThickness,
                Config.CaretThickness,
                Config.CaretThickness + TextProperties.CharDimensions.Width,
                Config.CaretThickness);
        }

        void prepareLineData()
        {
            lines.CollectionChanged += onLinesCollectionChanged;
            lines.Add(createNewTextLine());
        }

        void onLinesCollectionChanged(
            object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                // Add the TextLine to our children.
                case NotifyCollectionChangedAction.Add:
                    var line = e.NewItems[0] as TextLine;
                    Children.Insert(e.NewStartingIndex, line);
                    break;

                // Remove the TextLine from our children.
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        Children.Remove(item as TextLine);
                    }
                    break;

                // All other actions are not supported.
                default:
                    throw new NotSupportedException(
                        "This action is not supported.");
            }
        }

        public bool CaretAtDocumentStart 
        { 
            get { return caret.AtDocumentStart; } 
        }

        public int CaretX { get { return caret.GridX; } }

        public void Translate(int offset)
        {
            caret.Offset(offset);
        }

        public void TranslateToDocumentStart()
        {
            caret.ToDocumentStart();
        }

        public void TranslateToDocumentEnd()
        {
            caret.ToDocumentEnd();
        }

        public void TranslateToLineStart()
        {
            caret.ToLineStart();
        }

        public void TranslateToLineEnd()
        {
            caret.ToLineEnd();
        }

        public void TranslateToPrevLine()
        {
            caret.ToPrevLine();
        }

        public void TranslateToNextLine()
        {
            caret.ToNextLine();
        }

        // Operation module.
        int operationStack = 0;

        bool operationInProgress { get { return operationStack > 0; } }

        public void StartOperation()
        {
            // Increment the operation stack.
            operationStack++;
        }

        public void EndOperation()
        {
            // Decrement the operation stack.
            operationStack = Math.Max(0, operationStack - 1);

            // If the operation has ended, update the caret display.
            if (!operationInProgress) updateCaretDisplay();
        }
        // End operation module.

        public string GetCurrentLineText()
        {
            return lines[caret.GridY].GetLineText();
        }

        public string GetFullText()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < lines.Count; i++)
            {
                if (i > 0) builder.Append(Environment.NewLine);
                builder.Append(lines[i].GetLineText());
            }
            return builder.ToString();
        }

        public void InsertLineBreak()
        {
            insertNewLine();
        }

        TextLine createNewTextLine()
        {
            return createNewTextline(new List<Segment>());
        }

        TextLine createNewTextline(IEnumerable<Segment> segments)
        {
            var textLine = new TextLine(segments);
            textLine.RenderFinished += (sender, e) => updateCaretDisplay();
            return textLine;
        }

        // Inserts a linebreak followed by the specified list of segments at
        // the caret position.
        void insertNewLine()
        {
            // Cache caret grid position.
            var y = caret.GridY;
            var x = caret.GridX;

            // Update caret grid position.
            caret.GridY++;
            caret.GridX = 0;

            // Split off the line into two lines at the caret position.
            splitLines(lines[y], x);
        }

        void splitLines(TextLine line, int segmentIndex)
        {
            // Start complex text operation.
            StartOperation();

            // Retrieve the segment data after the index to be split.
            var newLineSegmentCount = line.SegmentData.Count - segmentIndex;
            var newLineSegments = line.SegmentData.GetRange(
                segmentIndex, 
                newLineSegmentCount);

            // Remove the segments from the original line that will be split 
            // off.
            line.SegmentData.RemoveRange(segmentIndex, newLineSegmentCount);

            // Create a new line with the split off segments and add it to the
            // document.
            var newLine = createNewTextline(newLineSegments);
            var lineIndex = lines.IndexOf(line);
            lines.Insert(lineIndex + 1, newLine);

            // End complex text operation.
            EndOperation();
        }

        public void InsertText(string text, Brush foreground)
        {
            InsertText(text, foreground, false, true);
        }

        public void InsertText(
            string text, 
            Brush foreground, 
            bool eachCharAsSegment)
        {
            InsertText(text, foreground, eachCharAsSegment, true);
        }

        public void InsertText(
            string text,
            Brush foreground,
            bool eachCharAsSegment,
            bool fade)
        {
            // Quit if the text is an empty string.
            if (string.IsNullOrEmpty(text)) return;

            // Animate the brush.
            if (fade) foreground = createTextBrush(foreground);

            // Insert the text.
            var segments = eachCharAsSegment
                ? getCharSegments(text, foreground)
                : getSingleSegment(text, foreground);
            insertSegments(segments);

            //ckgtest Use this code for to convert \r\n to linebreaks?
            //var delimiter = new string[] { "\r\n" };
            //var lines = text.Split(delimiter, StringSplitOptions.None);
            //for (var i = 0; i < lines.Length; i++)
            //{
            //    // Insert the line text.
            //    var line = lines[i];
            //    var segments = eachCharAsSegment
            //        ? getCharSegments(line, foreground)
            //        : getSingleSegment(line, foreground);
            //    insertSegments(segments);

            //    // Insert a line break if there are more lines following this.
            //    if (i + 1 < lines.Length) insertNewLine();
            //}
        }

        ICollection<Segment> getSingleSegment(string text, Brush foreground)
        {
            var segment = new Segment(text, foreground);
            return new List<Segment> { segment };
        }

        ICollection<Segment> getCharSegments(string text, Brush foreground)
        {
            // Build linked list of character segments.
            var charList = new LinkedList<Segment>();
            foreach (var c in text)
            {
                var segment = new Segment(c.ToString(), foreground);
                charList.AddLast(segment);
            }
            return charList;
        }

        void insertSegments(ICollection<Segment> segments)
        {
            // Cache caret grid position.
            var x = caret.GridX;
            var y = caret.GridY;

            // Update caret grid position.
            caret.GridX += segments.Count;

            // Update segment data.
            lines[y].SegmentData.InsertRange(x, segments);
        }

        public void DeleteToEnd()
        {
            DeleteSegments(int.MaxValue);
        }

        public void DeleteSegments(int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (caret.AtDocumentEnd) break;
                deleteCurrentSegment();
            }
        }

        void deleteCurrentSegment()
        {
            if (caret.AtLineEnd) deleteLineBreak();
            else deleteTextSegment();
        }

        void deleteTextSegment()
        {
            lines[caret.GridY].SegmentData.RemoveAt(caret.GridX);
        }

        void deleteLineBreak()
        {
            // Quit if we are at the end of the document.
            if (caret.AtLastLine) return;

            // Combine the current line with the subsequent one.
            var currentLine = lines[caret.GridY];
            var lineAfter = lines[caret.GridY + 1];
            combineLines(currentLine, lineAfter);
        }

        void combineLines(TextLine baseLine, TextLine subLine)
        {
            baseLine.SegmentData.AddRange(subLine.SegmentData);
            lines.Remove(subLine);
        }

        static Brush createTextBrush(Brush baseBrush)
        {
            var animation = new BrushAnimation();
            return animation.GetFadingBrush(
                baseBrush,
                Config.TextFadeDuration);
        }

        void updateCaretDisplay()
        {
            // Do not update the caret display if an operation is in progress.
            if (operationInProgress) return;

            // Fire caret moved event.
            var currentLine = lines[caret.GridY];
            var args = new CaretMovedEventArgs(currentLine, caret.GridX);
            CaretMoved?.Invoke(this, args);
        }
    }
}