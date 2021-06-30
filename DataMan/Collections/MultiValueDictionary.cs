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

namespace Baxendale.DataManagement.Collections
{
    public class MultiValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public IEqualityComparer<TValue> ValueComparer { get; }

        private Dictionary<TKey, ISet<TValue>> _dictionary;

        public MultiValueDictionary()
            : this(0, null, null)
        {
        }

        public MultiValueDictionary(int capacity)
            : this(capacity, null, null)
        {
        }

        public MultiValueDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : this(0, keyComparer, valueComparer)
        {
        }

        public MultiValueDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null, null)
        {
        }

        public MultiValueDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _dictionary = new Dictionary<TKey, ISet<TValue>>(keyComparer);
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            foreach (KeyValuePair<TKey, TValue> kv in dictionary)
                Add(kv.Key, kv.Value);
        }

        public MultiValueDictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _dictionary = new Dictionary<TKey, ISet<TValue>>(capacity, keyComparer);
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        public int Count
        {
            get
            {
                return _dictionary.Select(o => o.Value.Count).Sum();
            }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            ISet<TValue> values;
            if (_dictionary.TryGetValue(item.Key, out values))
                return values.Contains(item.Value);
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach(KeyValuePair<TKey, TValue> kv in this)
                array[arrayIndex++] = kv;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _dictionary.Values.Select(a => a.AsEnumerable()).Aggregate((a, b) => a.Concat(b)).ToArray();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            ISet<TValue> values;
            if (!_dictionary.TryGetValue(key, out values))
            {
                values = new HashSet<TValue>(ValueComparer);
                _dictionary[key] = values;
            }
            values.Add(value);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool Remove(TKey key, TValue value)
        {
            ISet<TValue> values;
            if (_dictionary.TryGetValue(key, out values))
            {
                if (values.Remove(value))
                {
                    if (values.Count == 0)
                        _dictionary.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                return _dictionary[key].AsReadOnly();
            }
        }

        public bool TryGetValue(TKey key, out IEnumerable<TValue> values)
        {
            ISet<TValue> set;
            if (_dictionary.TryGetValue(key, out set))
            {
                values = set.ToArray();
                return true;
            }
            values = null;
            return false;
        }

        #region IDictionary<TKey, TValue>

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return _dictionary[key].First();
            }
            set
            {
                Add(key, value);
            }
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            ISet<TValue> set;
            if (_dictionary.TryGetValue(key, out set))
            {
                value = set.First();
                return true;
            }
            value = default(TValue);
            return false;
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

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key, item.Value);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, ISet<TValue>> kv in _dictionary)
            {
                foreach (TValue value in kv.Value)
                    yield return new KeyValuePair<TKey, TValue>(kv.Key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
