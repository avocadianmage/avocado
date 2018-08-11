using System;

namespace AvocadoLib.Basic
{
    public static class StringExtensions
    {
        // Displays a number with a fixed number of decimal places.
        public static string ToRoundedString(this double num, int decimals)
        {
            var formatter = $"0.{ new string('0', decimals) }";
            return Math.Round(num, decimals).ToString(formatter);
        }
    }
}