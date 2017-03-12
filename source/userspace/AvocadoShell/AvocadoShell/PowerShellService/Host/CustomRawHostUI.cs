using System;
using System.Management.Automation.Host;

namespace AvocadoShell.PowerShellService.Host
{
    sealed class CustomRawHostUI : PSHostRawUserInterface
    {
        /// <summary>
        /// Gets or sets the background color of text to be written.
        /// </summary>
        public override ConsoleColor BackgroundColor
        {
            get { return default(ConsoleColor); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the host buffer size.
        /// </summary>
        public override Size BufferSize { get; set; }
        
        /// <summary>
        /// Gets or sets the cursor position.
        /// </summary>
        public override Coordinates CursorPosition
        {
            get { return default(Coordinates); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the cursor size.
        /// </summary>
        public override int CursorSize
        {
            get { return default(int); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the foreground color of the text to be written.
        /// </summary>
        public override ConsoleColor ForegroundColor
        {
            get { return Config.SystemConsoleForeground; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether a key is available.
        /// </summary>
        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the maximum physical size of the window.
        /// </summary>
        public override Size MaxPhysicalWindowSize
        {
            get { return default(Size); } // Not implemented.
        }

        /// <summary>
        /// Gets the maximum window size.
        /// </summary>
        public override Size MaxWindowSize
        {
            get { return default(Size); } // Not implemented.
        }

        /// <summary>
        /// Gets or sets the window position.
        /// </summary>
        public override Coordinates WindowPosition
        {
            get { return default(Coordinates); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the window size.
        /// </summary>
        public override Size WindowSize
        {
            get { return default(Size); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        public override string WindowTitle
        {
            get { return default(string); } // Not implemented.
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Resets the input buffer.
        /// </summary>
        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a rectangular region of the screen buffer.
        /// </summary>
        /// <param name="rectangle">Defines the size of the region.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a pressed, released, or pressed and released keystroke from
        /// the keyboard device, blocking processing until a keystroke is typed
        /// that matches the specified keystroke options.
        /// </summary>
        /// <param name="options">Options, such as IncludeKeyDown, used when 
        /// reading the keyboard.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Crops a region of the screen buffer.
        /// </summary>
        /// <param name="source">The region of the screen to be scrolled.
        /// </param>
        /// <param name="destination">The region of the screen to receive the 
        /// source region contents.</param>
        /// <param name="clip">The region of the screen to include in the 
        /// operation.</param>
        /// <param name="fill">The character and attributes to be used to fill 
        /// all cells.</param>
        public override void ScrollBufferContents(
            Rectangle source, 
            Coordinates destination, 
            Rectangle clip, 
            BufferCell fill)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies an array of buffer cells into the screen buffer at a 
        /// specified location.
        /// </summary>
        /// <param name="origin">Not used.</param>
        /// <param name="contents">Not used.</param>
        public override void SetBufferContents(
            Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies a given character, foreground color, and background color
        /// to a region of the screen buffer.</summary>
        /// <param name="rectangle">Defines the area to be filled.</param>
        /// <param name="fill">Defines the fill character.</param>
        public override void SetBufferContents(
            Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }
    }
}