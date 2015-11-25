using System;
using System.Windows;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.TextRendering
{
    public sealed class LabelTextArea : TextArea
    {
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(LabelTextArea),
                new FrameworkPropertyMetadata());

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty AnimateTextProperty
            = DependencyProperty.Register(
                "AnimateText",
                typeof(bool),
                typeof(LabelTextArea),
                new FrameworkPropertyMetadata(true));

        public bool AnimateText
        {
            get { return (bool)GetValue(AnimateTextProperty); }
            set { SetValue(AnimateTextProperty, value); }
        }

        public LabelTextArea()
        {
            Margin = new Thickness(0, -1, 0, 0);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            renderText();
            this.RegisterPropertyOnChange(TextProperty, onRefresh);
        }

        void onRefresh(object sender, EventArgs e) => renderText();

        void renderText()
        {
            Clear();
            Write(Text, Foreground);
        }
    }
}