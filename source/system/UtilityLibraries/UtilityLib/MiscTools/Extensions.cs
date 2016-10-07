using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.MiscTools
{
    public static class Extensions
    {
        // Returns the common substring of the start of the two strings.
        public static string CommonStart(this string str, string strToCompare)
        {
            var commonSubstring = string.Empty;

            var count = Math.Min(str.Length, strToCompare.Length);
            for (var i = 0; i < count; i++)
            {
                var c1 = str[i];
                var c2 = strToCompare[i];
                if (c1 != c2) break;
                commonSubstring += c1;
            }

            return commonSubstring;
        }

        // Format this string so that it can be used in URIs.
        public static string ToUriCompatible(this string str)
        {
            const char SEPARATOR = '-';
            var specialChars = new char[] { ' ', ':', '*', '/', '(', ')' };

            // Set to lowercase.
            str = str.ToLower();

            // Replace special characters with the URL-friendly separator.
            specialChars.ForEach(c => str = str.Replace(c, SEPARATOR));

            // Eliminate any duplicate, adjacent separators.
            for (var i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == SEPARATOR && str[i] != str[i + 1]) continue;
                str = str.Remove(i--, 1);
            }

            // Remove separators at the ends and return.
            return str.Trim(SEPARATOR);
        }

        // Displays a number with a fixed number of decimal places.
        public static string ToRoundedString(this double num, int decimals)
        {
            var formatter = $"0.{ new string('0', decimals) }";
            return Math.Round(num, decimals).ToString(formatter);
        }

        public static string Base64Encode(this string str)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

        public static string Base64Decode(this string str)
            => Encoding.UTF8.GetString(Convert.FromBase64String(str));

        public static void RunAsync(this Task task)
        {
            if (task.Status == TaskStatus.Created) task.Start();
        }

        public static IEnumerable<T> ForEach<T>(
            this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var i in enumerable) action(i);
            return enumerable;
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
