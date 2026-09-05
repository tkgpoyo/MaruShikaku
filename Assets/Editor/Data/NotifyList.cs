using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace MaruSikaku.Editor.Data
{
    public sealed class ItemPropertyChangedEventArgs<T> : EventArgs
    {
        public T Item { get; }
        public string PropertyName { get; }

        public ItemPropertyChangedEventArgs(T item, string propertyName)
        {
            Item = item;
            PropertyName = propertyName;
        }
    }

    public class NotifyList<T> : IList<T>, IReadOnlyList<T>, INotifyBindablePropertyChanged
    {
        public const string ITEM_PROPERTY = "Item";
        public const string ITEMS_PROPERTY = "Items";

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        public event EventHandler<ItemPropertyChangedEventArgs<T>> itemPropertyChanged;

        private List<T> _items = new();

        public T this[int index]
        {
            get => _items[index];
            set
            {
                if (EqualityComparer<T>.Default.Equals(_items[index], value)) { return; }
                Unsubscribe(_items[index]);
                _items[index] = value;
                Subscribe(_items[index]);
                NotifyListChanged();
            }
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
            foreach (var item in _items)
            {
                Subscribe(item);
            }
        }

        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => _items.IndexOf(item);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _items.Add(item);
            Subscribe(item);
            NotifyListChanged();
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                Unsubscribe(item);
            }
            _items.Clear();
            NotifyListChanged();
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            Subscribe(item);
            NotifyListChanged();
        }

        public bool Remove(T item)
        {
            var removed = _items.Remove(item);
            if (removed)
            {
                Unsubscribe(item);
                NotifyListChanged();
            }
            return removed;
        }

        public void RemoveAt(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            Unsubscribe(item);
            NotifyListChanged();
        }

        /// <summary>
        /// 変更通知を行います．
        /// </summary>
        /// <param name="property">プロパティ名</param>
        private void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new(property));
        }

        private void NotifyListChanged()
        {
            Notify(nameof(Count));
            Notify(ITEMS_PROPERTY);
        }

        private void Subscribe(T item)
        {
            if (item is INotifyBindablePropertyChanged notifyItem)
            {
                notifyItem.propertyChanged += OnItemChanged;
            }
        }

        private void Unsubscribe(T item)
        {
            if (item is INotifyBindablePropertyChanged notifyItem)
            {
                notifyItem.propertyChanged -= OnItemChanged;
            }
        }

        private void OnItemChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (sender is T item)
            {
                itemPropertyChanged?.Invoke(this, new(item, e.propertyName));
            }
            Notify(ITEM_PROPERTY);
            Notify($"{ITEMS_PROPERTY}.{e.propertyName}");
        }
    }
}
