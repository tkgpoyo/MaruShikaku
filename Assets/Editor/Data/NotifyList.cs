using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Data
{
    public class NotifyList<T> : IList<T>, INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private List<T> _items = new();

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public NotifyList()
        {
            _items = new();
        }
        public NotifyList(IEnumerable<T> items)
        {
            _items = items != null ? new List<T>(items) : new List<T>();
        }


        public void Add(T item)
        {
            _items.Add(item);
            Notify();
        }

        public void Clear()
        {
            _items.Clear();
            Notify();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            Notify();
        }

        public bool Remove(T item)
        {
            var removed = _items.Remove(item);
            if (removed) { Notify(); }
            return removed;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            Notify();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }
    }
}