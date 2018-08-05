using AvocadoLib.CommandLine.ANSI;
using System.Collections.Generic;
using System.Windows.Media;

namespace AvocadoServiceHost.Utilities
{
    enum ColorType
    {
        None,
        Timestamp,
        KeyPhrase,
        ThisMachineIdentifier = 3,
        LANIdentifier = 4,
        ExternalIdentifier = 5
    }

    static class TextColorPalette
    {
        static readonly Dictionary<ColorType, Color> colorDict
            = new Dictionary<ColorType, Color>
            {
                { ColorType.Timestamp, Colors.DimGray },
                { ColorType.KeyPhrase, Color.FromRgb(86, 156, 214) },
                { ColorType.ThisMachineIdentifier, Colors.LightGreen },
                { ColorType.LANIdentifier, Colors.Yellow },
                { ColorType.ExternalIdentifier, Colors.Orange}
            };

        public static string GetColorString(ColorType type, string text)
        {
            return type == ColorType.None
                ? text : ANSICode.GetColoredText(colorDict[type], text);
        }
    }
}
