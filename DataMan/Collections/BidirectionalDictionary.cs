﻿//
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
    public abstract class BidirectionalDictionary<TKey, TValue, TReverseDict> : IBidirectionalDictionary<TKey, TValue>
        where TReverseDict : IBidirectionalDictionary<TValue, TKey>
    {
        protected abstract IDictionary<TKey, TValue> KeyValueDictionary { get; }
        protected abstract IDictionary<TValue, TKey> ValueKeyDictionary { get; }

        public abstract int Count { get; }

        public virtual ICollection<TKey> Keys => KeyValueDictionary.Keys;
        public virtual ICollection<TValue> Values => ValueKeyDictionary.Keys;

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

        public abstract void Add(TKey key, TValue value);

        public abstract TReverseDict AsReverse();

        IBidirectionalDictionary<TValue, TKey> IBidirectionalDictionary<TKey, TValue>.AsReverse()
        {
            return AsReverse();
        }

        public abstract void Clear();

        public virtual bool ContainsKey(TKey key)
        {
            return KeyValueDictionary.ContainsKey(key);
        }

        public virtual bool ContainsValue(TValue value)
        {
            return ValueKeyDictionary.ContainsKey(value);
        }

        public abstract TValue GetValueByKey(TKey key);

        public abstract TKey GetKeyByValue(TValue value);

        public abstract bool RemoveByKey(TKey key);

        public abstract bool RemoveByValue(TValue value);

        public abstract void SetKeyByValue(TValue value, TKey newKey);

        public abstract void SetValueByKey(TKey key, TValue newValue);

        public virtual bool TryGetKey(TValue value, out TKey key)
        {
            return ValueKeyDictionary.TryGetValue(value, out key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return KeyValueDictionary.TryGetValue(key, out value);
        }

        public virtual Dictionary<TKey, TValue> ToDictionary()
        {
            return new Dictionary<TKey, TValue>(this);
        }

        public static explicit operator TReverseDict(BidirectionalDictionary<TKey, TValue, TReverseDict> dict)
        {
            return dict.AsReverse();
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

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return KeyValueDictionary.Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if ((uint)(arrayIndex + Count) >= array.Length) throw new ArgumentOutOfRangeException(nameof(array));
            
            foreach (KeyValuePair<TKey, TValue> kv in this)
                array[arrayIndex++] = kv;
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