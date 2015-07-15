using System.Windows;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.Text
{
    public abstract class TextControl : BaseControl
    {
        static TextControl()
        {
            // Associate this window with the default theme.
            var frameType = typeof(TextControl);
            DefaultStyleKeyProperty.OverrideMetadata(
                frameType,
                new FrameworkPropertyMetadata(frameType));
        }

        protected TextContainer TextContent
        {
            get 
            {
                return this.GetTemplateElement<TextContainer>("textContent"); 
            }
        }

        protected void WriteLine(string text, Brush foreground)
        {
            TextContent.InsertText(text, foreground);
            TextContent.InsertLineBreak();
        }
    }
}
