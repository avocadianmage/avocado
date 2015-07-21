using System;

namespace AvocadoFramework.Controls.Text
{
    sealed class CaretMovedEventArgs : EventArgs
    {
        public TextLine ParentTextLine { get; }
        public int GridX { get; }

        public CaretMovedEventArgs(TextLine parentTextLine, int gridX)
        {
            ParentTextLine = parentTextLine;
            GridX = gridX;
        }
    }
}