using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace StandardLibrary.Utilities
{
    public sealed class ObservableList<T> : IEnumerable<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        readonly List<T> data = new List<T>();

        public void Notify() => CollectionChanged?.Invoke(this, null);

        public int Count => data.Count;

        public T Insert(int index, T item, bool notify = true)
        {
            data.Insert(index, item);
            if (notify)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, index);
                CollectionChanged?.Invoke(this, args);
            }
            return item;
        }

        public IEnumerable<T> RemoveAll(Predicate<T> match, bool notify = true)
        {
            var itemsToRemove = data.Where(match.Invoke);
            data.RemoveAll(match);
            if (notify)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, itemsToRemove);
                CollectionChanged?.Invoke(this, args);
            }
            return itemsToRemove;
        }

        public IEnumerable<T> RemoveRange(
            int index, int count, bool notify = true)
        {
            var itemsToRemove = data.Take(index + count).Skip(index);
            data.RemoveRange(index, count);
            if (notify)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, itemsToRemove, index);
                CollectionChanged?.Invoke(this, args);
            }
            return itemsToRemove;
        }

        public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    }
}
