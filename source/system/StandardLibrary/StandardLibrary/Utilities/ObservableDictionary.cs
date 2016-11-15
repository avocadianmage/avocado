﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StandardLibrary.Utilities
{
    public sealed class ObservableDictionary<TKey, TValue>
    {
        public IEnumerable<TValue> EnumerableData => observableCollection;

        readonly ObservableCollection<TValue> observableCollection
            = new ObservableCollection<TValue>();
        readonly Dictionary<TKey, TValue> dictionary 
            = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] => dictionary[key];

        public void Add(TKey key, TValue value)
        {
            observableCollection.Add(value);
            dictionary.Add(key, value);
        }
    }
}