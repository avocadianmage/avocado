using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls.TextRendering
{
    public class TextLabel : Label
    {
        static TextLabel()
        {
            // Associate this control with the default theme.
            var type = typeof(TextLabel);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }
    }
}
