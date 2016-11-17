﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StandardLibrary.Utilities
{
    public sealed class ObservableDictionary<TKey, TValue> 
        : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public IEnumerable<TValue> EnumerableData => observableCollection;

        readonly ObservableCollection<TValue> observableCollection
            = new ObservableCollection<TValue>();
        readonly Dictionary<TKey, TValue> dictionary 
            = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] => dictionary[key];
        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public void Add(TKey key, TValue value)
        {
            observableCollection.Add(value);
            dictionary.Add(key, value);
        }

        public void Remove(TKey key)
        {
            observableCollection.Remove(dictionary[key]);
            dictionary.Remove(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() 
            => dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();
    }
}