using System;
using System.Windows;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.Text
{
    public sealed class SimpleLabel : TextContainer
    {
        public static readonly DependencyProperty ContentProperty
            = DependencyProperty.Register(
                "Content",
                typeof(string),
                typeof(SimpleLabel),
                new FrameworkPropertyMetadata());

        public string Content
        {
            get { return GetValue(ContentProperty) as string; }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty TextBrushProperty
            = DependencyProperty.Register(
                "TextBrush",
                typeof(Brush),
                typeof(SimpleLabel),
                new FrameworkPropertyMetadata(Brushes.White));

        public Brush TextBrush
        {
            get { return GetValue(TextBrushProperty) as Brush; }
            set { SetValue(TextBrushProperty, value); }
        }

        public static readonly DependencyProperty FadeProperty
            = DependencyProperty.Register(
                "Fade",
                typeof(bool),
                typeof(SimpleLabel),
                new FrameworkPropertyMetadata(true));

        public bool Fade
        {
            get { return (bool)GetValue(FadeProperty); }
            set { SetValue(FadeProperty, value); }
        }

        public SimpleLabel()
        {
            this.RegisterPropertyOnChange(ContentProperty, onRefresh);
            this.RegisterPropertyOnChange(TextBrushProperty, onRefresh);
        }

        void onRefresh(object sender, EventArgs e)
        {
            renderText();
        }

        void renderText()
        {
            TranslateToDocumentStart();
            DeleteToEnd();
            InsertText(Content, TextBrush, false, Fade);
        }
    }
}