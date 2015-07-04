using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace AvocadoFramework.Controls.Text
{
    static class TextProperties
    {
        public static Size CharDimensions { get { return charDimensions; } }

        static readonly Size charDimensions;

        static TextProperties()
        {
            charDimensions = getCharDimensions();
        }

        public static FormattedText CreateFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                Config.Typeface,
                Config.FontSize,
                Brushes.Transparent,
                null,
                TextFormattingMode.Display);
        }

        static Size getCharDimensions()
        {
            const string MODEL_CHAR = "X";
            var formattedText = CreateFormattedText(MODEL_CHAR);
            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}