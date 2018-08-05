using System.Collections.Generic;
using System.Windows.Media;

namespace AvocadoServiceHost.Utilities
{
    enum ColorType
    {
        None,
        Timestamp,
        KeyPhrase
    }

    static class TextColorPalette
    {
        static readonly Dictionary<ColorType, Color> colorDict
            = new Dictionary<ColorType, Color>
            {
                { ColorType.Timestamp, Colors.DimGray },
                { ColorType.KeyPhrase, Color.FromRgb(86, 156, 214) }
            };

        public static Color GetColor(ColorType type) => colorDict[type];
    }
}
