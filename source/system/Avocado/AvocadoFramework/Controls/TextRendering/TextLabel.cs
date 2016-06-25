﻿using System.Windows;
using System.Windows.Controls;

namespace AvocadoFramework.Controls.TextRendering
{
    public class TextLabel : Control
    {
        static TextLabel()
        {
            // Associate this control with the default theme.
            var type = typeof(TextLabel);
            DefaultStyleKeyProperty.OverrideMetadata(
                type,
                new FrameworkPropertyMetadata(type));
        }

        public static readonly DependencyProperty ContentProperty
            = DependencyProperty.Register(
                "Content",
                typeof(string),
                typeof(TextLabel),
                new FrameworkPropertyMetadata());

        public string Content
        {
            get { return GetValue(ContentProperty) as string; }
            set { SetValue(ContentProperty, value); }
        }
    }
}
