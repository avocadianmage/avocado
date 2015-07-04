using System;

namespace AvocadoFramework.Controls.Text
{
    sealed class CaretMovedEventArgs : EventArgs
    {
        public TextLine ParentTextLine { get { return parentTextLine; } }
        public int GridX { get { return gridX; } }

        readonly TextLine parentTextLine;
        readonly int gridX;

        public CaretMovedEventArgs(TextLine parentTextLine, int gridX)
        {
            this.parentTextLine = parentTextLine;
            this.gridX = gridX;
        }
    }
}