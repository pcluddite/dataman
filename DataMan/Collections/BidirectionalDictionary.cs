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
        public abstract void Clear();

        public virtual ICollection<TKey> Keys
        {
            get
            {
                return KeyValueDictionary.Keys;
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                return ValueKeyDictionary.Keys;
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                return GetValueByKey(key);
            }
            set
            {
                SetValueByKey(key, value);
            }
        }

        public abstract TValue GetValueByKey(TKey key);

        public abstract TKey GetKeyByValue(TValue value);

        public abstract void SetValueByKey(TKey key, TValue newValue);

        public abstract void SetKeyByValue(TValue value, TKey newKey);

        public abstract void Add(TKey key, TValue value);

        public abstract bool RemoveByKey(TKey key);

        public abstract bool RemoveByValue(TValue value);

        public virtual bool ContainsKey(TKey key)
        {
            return KeyValueDictionary.ContainsKey(key);
        }

        public virtual bool ContainsValue(TValue value)
        {
            return ValueKeyDictionary.ContainsKey(value);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return KeyValueDictionary.TryGetValue(key, out value);
        }

        public virtual bool TryGetKey(TValue value, out TKey key)
        {
            return ValueKeyDictionary.TryGetValue(value, out key);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return KeyValueDictionary.Contains(item);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (RemoveByKey(item.Key))
            {
                RemoveByValue(item.Value);
                return true;
            }
            return false;
        }

        #region IDictionary<TKey, TValue>

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return RemoveByKey(key);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach (KeyValuePair<TKey, TValue> kv in KeyValueDictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
            }
        }

        public virtual void CopyTo(KeyValuePair<TValue, TKey>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach (KeyValuePair<TValue, TKey> kv in ValueKeyDictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TValue, TKey>(kv.Key, kv.Value);
            }
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