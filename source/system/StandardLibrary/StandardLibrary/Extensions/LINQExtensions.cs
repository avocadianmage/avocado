using System;
using System.Collections.Generic;

namespace StandardLibrary.Extensions
{
    public static class LinqExtensions
    {
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