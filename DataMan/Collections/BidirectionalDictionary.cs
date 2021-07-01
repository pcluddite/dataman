//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021 Timothy Baxendale
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
//    USA
//
using System;
using System.Collections;
using System.Collections.Generic;

namespace Baxendale.DataManagement.Collections
{
    public abstract class BidirectionalDictionary<TKey, TValue> : IBidirectionalDictionary<TKey, TValue>
    {
        protected abstract IDictionary<TKey, TValue> KeyValueDictionary { get; }
        protected abstract IDictionary<TValue, TKey> ValueKeyDictionary { get; }

        public abstract int Count { get; }

        public virtual ICollection<TKey> Keys
        {
            get
            {
                lock (SyncRoot) return KeyValueDictionary.Keys;
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                lock (SyncRoot) return ValueKeyDictionary.Keys;
            }
        }

        protected abstract object SyncRoot { get; }

        public virtual TValue this[TKey key]
        {
            get
            {
                lock (SyncRoot) return GetValueByKey(key);
            }
            set
            {
                lock (SyncRoot) SetValueByKey(key, value);
            }
        }

        public abstract void Add(TKey key, TValue value);

        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            lock(SyncRoot)
            {
                foreach(KeyValuePair<TKey, TValue> kv in collection)
                {
                    KeyValueDictionary.Add(kv.Key, kv.Value);
                    ValueKeyDictionary.Add(kv.Value, kv.Key);
                }
            }
        }

        public abstract BidirectionalDictionary<TValue, TKey> AsReverse();

        public abstract void Clear();

        public virtual bool ContainsKey(TKey key)
        {
            lock (SyncRoot) return KeyValueDictionary.ContainsKey(key);
        }

        public virtual bool ContainsValue(TValue value)
        {
            lock (SyncRoot) return ValueKeyDictionary.ContainsKey(value);
        }

        public abstract TValue GetValueByKey(TKey key);

        public abstract TKey GetKeyByValue(TValue value);

        public abstract bool RemoveByKey(TKey key);

        public abstract bool RemoveByValue(TValue value);

        public abstract void SetKeyByValue(TValue value, TKey newKey);

        public abstract void SetValueByKey(TKey key, TValue newValue);

        public virtual bool TryGetKey(TValue value, out TKey key)
        {
            lock (SyncRoot) return ValueKeyDictionary.TryGetValue(value, out key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            lock (SyncRoot) return KeyValueDictionary.TryGetValue(key, out value);
        }

        public virtual Dictionary<TKey, TValue> ToDictionary()
        {
            lock (SyncRoot) return new Dictionary<TKey, TValue>(this);
        }

        public static explicit operator BidirectionalDictionary<TValue, TKey> (BidirectionalDictionary<TKey, TValue> dict)
        {
            return dict.AsReverse();
        }

        IBidirectionalDictionary<TValue, TKey> IBidirectionalDictionary<TKey, TValue>.AsReverse()
        {
            return AsReverse();
        }

        #region IDictionary<TKey, TValue>

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return RemoveByKey(key);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            lock(SyncRoot) return KeyValueDictionary.Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int count = Count;
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if ((uint)(arrayIndex + count) >= array.Length) throw new ArgumentOutOfRangeException(nameof(array));

            lock (SyncRoot)
            {
                foreach (KeyValuePair<TKey, TValue> kv in this)
                    array[arrayIndex++] = kv;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (RemoveByKey(item.Key))
            {
                RemoveByValue(item.Value);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}