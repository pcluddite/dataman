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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxendale.DataManagement.Collections
{
    public class BidirectionalDictionary<TKey, TValue> : IBidirectionalDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _first;
        private Dictionary<TValue, TKey> _second;

        public BidirectionalDictionary()
            : this(0, null, null)
        {
        }

        public BidirectionalDictionary(int capacity)
            : this(capacity, null, null)
        {
        }

        public BidirectionalDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : this(0, keyComparer, valueComparer)
        {
        }

        public BidirectionalDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null, null)
        {
        }

        public BidirectionalDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new Dictionary<TKey, TValue>(dictionary.Count, keyComparer);
            _second = new Dictionary<TValue, TKey>(dictionary.Count, valueComparer);
            foreach(KeyValuePair<TKey, TValue> kv in dictionary)
            {
                Add(kv.Key, kv.Value);
            }
        }

        public BidirectionalDictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new Dictionary<TKey, TValue>(capacity, keyComparer);
            _second = new Dictionary<TValue, TKey>(capacity, valueComparer);
        }

        private BidirectionalDictionary(Dictionary<TKey, TValue> first, Dictionary<TValue, TKey> second)
        {
            _first = first;
            _second = second;
        }

        public int Count
        {
            get
            {
                return _first.Count;
            }
        }

        public void Clear()
        {
            _first.Clear();
            _second.Clear();
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _first.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _second.Keys;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return _first[key];
            }
            set
            {
                _first[key] = value;
                _second[value] = key;
            }
        }

        public TValue GetValueByKey(TKey key)
        {
            return _first[key];
        }

        public TKey GetKeyByValue(TValue value)
        {
            return _second[value];
        }

        public void SetValueByKey(TKey key, TValue newValue)
        {
            TValue oldValue;
            if (!_first.TryGetValue(key, out oldValue))
                throw new KeyNotFoundException();
            _second.Remove(oldValue);

            TKey oldKey;
            if (_second.TryGetValue(newValue, out oldKey))
                _first.Remove(oldKey);

            _first[key] = newValue;
            _second[newValue] = key;
        }

        public void SetKeyByValue(TValue value, TKey newKey)
        {
            TKey oldKey;
            if (!_second.TryGetValue(value, out oldKey))
                throw new KeyNotFoundException();
            _first.Remove(oldKey);

            TValue oldValue;
            if (_first.TryGetValue(newKey, out oldValue))
                _second.Remove(oldValue);

            _second[value] = newKey;
            _first[newKey] = value;
        }

        public bool ContainsKey(TKey key)
        {
            return _first.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _second.ContainsKey(value);
        }

        public void Add(TKey key, TValue value)
        {
            _first.Add(key, value);
            _second.Add(value, key);
        }

        public bool RemoveByKey(TKey key)
        {
            TValue value;
            if (!_first.TryGetValue(key, out value))
                return false;
            if (_first.Remove(key))
            {
                if (_second.Remove(value))
                    return true;
                _first.Add(key, value);
            }
            return false;
        }

        public bool RemoveByValue(TValue value)
        {
            TKey key;
            if (!_second.TryGetValue(value, out key))
                return false;
            if (_second.Remove(value))
            {
                if (_first.Remove(key))
                    return true;
                _second.Add(value, key);
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _first.TryGetValue(key, out value);
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return _second.TryGetValue(value, out key);
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
            return ((ICollection<KeyValuePair<TKey, TValue>>)_first).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_first).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)_first).Remove(item))
            {
                if (((ICollection<KeyValuePair<TValue, TKey>>)_second).Remove(new KeyValuePair<TValue, TKey>(item.Value, item.Key)))
                    return true;
                ((ICollection<KeyValuePair<TKey, TValue>>)_first).Add(item);
            }
            return false;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _first.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implicit Operators

        public static implicit operator BidirectionalDictionary<TValue, TKey>(BidirectionalDictionary<TKey, TValue> o)
        {
            return new BidirectionalDictionary<TValue, TKey>(o._second, o._first);
        }

        #endregion
    }
}
