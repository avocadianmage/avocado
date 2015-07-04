using System;
using System.Collections.Generic;
using UtilityLib.MiscTools;

namespace AvocadoFramework.Controls.Text
{
    sealed class CaretMarker
    {
        internal event EventHandler<CaretMovedEventArgs> Translated;

        readonly IReadOnlyList<TextLine> listBinding;
        
        internal CaretMarker(IReadOnlyList<TextLine> listBinding)
        {
            this.listBinding = listBinding;
        }

        internal int GridX
        {
            get { return gridX; }
            set
            {
                gridX = value;
                if (allowCaching) cachedGridX = value;
            }
        }
        internal int GridY { get; set; }

        int gridX;

        // Flag indicating whether cachedGridX should be updated whenever
        // GridX is updated.
        bool allowCaching = true;

        // The maximum value of GridX while navigating vertically.
        int cachedGridX;

        internal bool AtDocumentStart
        {
            get { return AtFirstLine && AtLineStart; }
        }

        internal bool AtDocumentEnd
        {
            get { return AtLastLine && AtLineEnd; }
        }

        internal bool AtFirstLine { get { return GridY == 0; } }
        internal bool AtLastLine 
        { 
            get { return GridY == listBinding.Count - 1; } 
        }

        internal bool AtLineStart { get { return GridX == 0; } }
        internal bool AtLineEnd
        {
            get { return GridX == getSegmentCountInLine(GridY); }
        }

        internal void Offset(int offset)
        {
            recursiveTranslate(offset);
            fireMovedEvent();
        }

        internal void ToDocumentStart()
        {
            toCoordinates(0, 0);
        }

        internal void ToDocumentEnd()
        {
            var y = listBinding.Count - 1;
            var x = getSegmentCountInLine(y);
            toCoordinates(x, y);
        }

        internal void ToLineStart()
        {
            toCoordinates(0, GridY);
        }

        internal void ToLineEnd()
        {
            var x = getSegmentCountInLine(GridY);
            toCoordinates(x, GridY);
        }

        internal void ToPrevLine()
        {
            if (!AtFirstLine) toLine(GridY - 1);
        }

        internal void ToNextLine()
        {
            if (!AtLastLine) toLine(GridY + 1);
        }

        int getSegmentCountInLine(int y)
        {
            return listBinding[y].SegmentData.Count;
        }

        void toLine(int y)
        {
            allowCaching = false;
            var x = Math.Min(cachedGridX, getSegmentCountInLine(y));
            toCoordinates(x, y);
            allowCaching = true;
        }

        void toCoordinates(int x, int y)
        {
            GridX = x;
            GridY = y;
            fireMovedEvent();
        }

        void fireMovedEvent()
        {
            var currentLine = listBinding[GridY];
            var args = new CaretMovedEventArgs(currentLine, GridX);
            Translated.SafeInvoke(this, args);
        }

        private void recursiveTranslate(int offset)
        {
            while (true)
            {
                var segmentsInCurrentLine = getSegmentCountInLine(GridY);

                // Perform translation within the same line.
                GridX += offset;

                // Handle translate left overflow.
                if (GridX < 0)
                {
                    // Check if the caret is at the start of the document.
                    if (AtFirstLine)
                    {
                        GridX = 0;
                        return;
                    }

                    // Move the caret to the end of the previous line.
                    offset = GridX;
                    GridY--;
                    GridX = getSegmentCountInLine(GridY);
                }

                // Handle translate right overflow.
                else if (GridX > segmentsInCurrentLine)
                {
                    // Check if the caret is at the end of the document.
                    if (AtLastLine)
                    {
                        GridX = segmentsInCurrentLine;
                        return;
                    }

                    // Move the caret to the start of the next line.
                    offset = GridX - segmentsInCurrentLine;
                    GridY++;
                    GridX = 0;
                }

                // We are finished if there is no overflow.
                else return;

                // Adjust the offset for the linebreak.
                offset = offset - Math.Sign(offset);
            }
        }
    }
}