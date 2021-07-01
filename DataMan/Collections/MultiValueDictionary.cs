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
    public class MultiValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>, 
        ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        protected IDictionary<TKey, ISet<TValue>> _dictionary;
        protected int? _count = null;

        public virtual IEqualityComparer<TKey> KeyComparer { get; }
        public virtual IEqualityComparer<TValue> ValueComparer { get; }
        
        public virtual int Count
        {
            get
            {
                if (_count == null)
                    _count = _dictionary.Select(o => o.Value.Count).Sum();
                return _count.Value;
            }
        }

        public virtual ICollection<TKey> Keys => _dictionary.Keys;
        public virtual IEnumerable<TValue> Values => _dictionary.Values.Flatten();

        public virtual IEnumerable<TValue> this[TKey key]
        {
            get
            {
                foreach (TValue value in _dictionary[key])
                    yield return value;
            }
        }

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

        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, null, null)
        {
        }

        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _dictionary = new Dictionary<TKey, ISet<TValue>>(keyComparer);
            KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            foreach (KeyValuePair<TKey, TValue> kv in collection)
                Add(kv.Key, kv.Value);
        }

        public MultiValueDictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _dictionary = new Dictionary<TKey, ISet<TValue>>(capacity, keyComparer);
            KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        public virtual bool Add(TKey key, TValue value)
        {
            ISet<TValue> values;
            if (_dictionary.TryGetValue(key, out values))
            {
                if (!values.Add(value))
                    return false;
                IncrementCount();
            }
            else
            {
                values = CreateValueSet(value);
                _dictionary.Add(key, values);
                IncrementCount(values.Count);
            }
            return true;
        }

        public virtual bool Add(TKey key, IEnumerable<TValue> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            ISet<TValue> set;
            if (_dictionary.TryGetValue(key, out set))
            {
                if (set.Overlaps(values))
                    return false;
                set.IntersectWith(values);
            }
            else
            {
                _dictionary.Add(key, CreateValueSet(values));
            }
            return true;
        }

        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> kv in collection)
                Add(kv.Key, kv.Value);
        }

        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> collection)
        {
            foreach (KeyValuePair<TKey, IEnumerable<TValue>> kv in collection)
                Add(kv.Key, kv.Value);
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
            _count = 0;
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            ISet<TValue> values;
            if (_dictionary.TryGetValue(item.Key, out values))
                return values.Contains(item.Value);
            return false;
        }

        public virtual bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if ((uint)(arrayIndex + Count) >= array.Length) throw new ArgumentOutOfRangeException(nameof(array));
            foreach (KeyValuePair<TKey, TValue> kv in this)
                array[arrayIndex++] = kv;
        }

        public virtual bool Remove(TKey key)
        {
            ISet<TValue> values;
            if (!_dictionary.TryGetValue(key, out values) || !_dictionary.Remove(key))
                return false;
            DecrementCount(values.Count);
            return true;
        }

        public virtual bool Remove(TKey key, TValue value)
        {
            ISet<TValue> values;
            if (!_dictionary.TryGetValue(key, out values) || !values.Remove(value))
                return false;
            if (values.Count == 0)
                _dictionary.Remove(key);
            DecrementCount();
            return true;
        }

        public virtual bool TryGetValue(TKey key, out IEnumerable<TValue> values)
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

        public virtual Dictionary<TKey, ICollection<TValue>> ToDictionary()
        {
            Dictionary<TKey, ICollection<TValue>> dict = new Dictionary<TKey, ICollection<TValue>>(_dictionary.Count, KeyComparer);
            foreach (KeyValuePair<TKey, ISet<TValue>> kv in _dictionary)
                dict.Add(kv.Key, kv.Value.ToArray());
            return dict;
        }

        public virtual Dictionary<TKey, TValue> Flatten()
        {
            Dictionary<TKey, TValue> other = new Dictionary<TKey, TValue>(_dictionary.Count, KeyComparer);
            foreach (KeyValuePair<TKey, ISet<TValue>> kv in _dictionary)
                other.Add(kv.Key, kv.Value.First());
            return other;
        }

        protected void IncrementCount()
        {
            IncrementCount(1);
        }

        protected void IncrementCount(int n)
        {
            if (_count != null) _count += n;
        }

        protected void DecrementCount()
        {
            DecrementCount(1);
        }

        protected void DecrementCount(int n)
        {
            if (_count != null) _count -= n;
        }

        protected virtual ISet<TValue> CreateValueSet(TValue value)
        {
            ISet<TValue> set = new HashSet<TValue>(ValueComparer);
            set.Add(value);
            return set;
        }

        protected virtual ISet<TValue> CreateValueSet(IEnumerable<TValue> values)
        {
            return new HashSet<TValue>(values, ValueComparer);
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

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToArray();

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!Add(key, value)) throw new ArgumentException("key/value combination already exists");
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

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, ISet<TValue>> kv in _dictionary)
            {
                foreach (TValue value in kv.Value)
                    yield return new KeyValuePair<TKey, TValue>(kv.Key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
