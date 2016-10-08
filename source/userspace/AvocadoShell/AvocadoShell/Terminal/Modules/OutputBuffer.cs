﻿using AvocadoFramework.Controls.TextRendering;
using System.Collections.Generic;
using System.Windows.Media;

namespace AvocadoShell.Terminal.Modules
{
    sealed class OutputBuffer
    {
        readonly Dictionary<Color, Brush> animatedBrushLookup
            = new Dictionary<Color, Brush>();

        string newlineBuffer;
        bool hitNonwhitespace;

        public OutputBuffer()
        {
            Reset();
        }

        public bool ProcessNewOutput(ref string text, ref Brush brush)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    if (hitNonwhitespace) newlineBuffer += "\r";
                    return false;
                }

                hitNonwhitespace = true;
                text = newlineBuffer + text;
                newlineBuffer = string.Empty;

                var colorKey = (brush as SolidColorBrush).Color;
                if (!animatedBrushLookup.ContainsKey(colorKey))
                {
                    animatedBrushLookup[colorKey] 
                        = TextArea.CreateTextFadeBrush(brush);
                }
                brush = animatedBrushLookup[colorKey];

                return true;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                newlineBuffer = string.Empty;
                hitNonwhitespace = false;
                animatedBrushLookup.Clear();
            }
        }
    }
}