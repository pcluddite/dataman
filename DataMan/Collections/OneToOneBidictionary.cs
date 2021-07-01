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
using System.Collections.Generic;

namespace Baxendale.DataManagement.Collections
{
    public sealed class OneToOneBidictionary<TKey, TValue> : BidirectionalDictionary<TKey, TValue, OneToOneBidictionary<TValue, TKey>>
    {
        private Dictionary<TKey, TValue> _first;
        private Dictionary<TValue, TKey> _second;
        private OneToOneBidictionary<TValue, TKey> _reverse;

        private readonly object _syncRoot;

        protected override IDictionary<TKey, TValue> KeyValueDictionary => _first;
        protected override IDictionary<TValue, TKey> ValueKeyDictionary => _second;
        protected override object SyncRoot => _syncRoot;

        public override int Count => _first.Count;

        public OneToOneBidictionary()
            : this(0, null, null)
        {
        }

        public OneToOneBidictionary(int capacity)
            : this(capacity, null, null)
        {
        }

        public OneToOneBidictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : this(0, keyComparer, valueComparer)
        {
        }

        public OneToOneBidictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, null, null)
        {
        }

        public OneToOneBidictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _syncRoot = new object();
            _first = new Dictionary<TKey, TValue>(keyComparer);
            _second = new Dictionary<TValue, TKey>(valueComparer);
            AddRange(collection);
        }

        public OneToOneBidictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _syncRoot = new object();
            _first = new Dictionary<TKey, TValue>(capacity, keyComparer);
            _second = new Dictionary<TValue, TKey>(capacity, valueComparer);
        }

        private OneToOneBidictionary(OneToOneBidictionary<TValue, TKey> reverse)
        {
            _syncRoot = reverse._syncRoot;
            _first = reverse._second;
            _second = reverse._first;
            _reverse = reverse;
        }

        public override void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                if (_second.ContainsKey(value))
                    throw new ArgumentException("Value is already assigned to a key", nameof(value));
                _first.Add(key, value);
                _second.Add(value, key);
            }
        }

        public override OneToOneBidictionary<TValue, TKey> AsReverse()
        {
            if (_reverse == null)
                lock (_syncRoot) _reverse = new OneToOneBidictionary<TValue, TKey>(this);
            return _reverse;
        }

        public override void Clear()
        {
            lock (_syncRoot)
            {
                _first.Clear();
                _second.Clear();
            }
        }

        public override TKey GetKeyByValue(TValue value)
        {
            lock (_syncRoot) return _second[value];
        }

        public override TValue GetValueByKey(TKey key)
        {
            lock (_syncRoot) return _first[key];
        }

        public override bool RemoveByKey(TKey key)
        {
            lock (_syncRoot)
            {
                TValue value;
                if (_first.TryGetValue(key, out value) && _first.Remove(key))
                {
                    if (_second.Remove(value))
                        return true;
                    _first.Add(key, value);
                }
                return false;
            }
        }

        public override bool RemoveByValue(TValue value)
        {
            lock (_syncRoot)
            {
                TKey key;
                if (_second.TryGetValue(value, out key) && _second.Remove(value))
                {
                    if (_first.Remove(key))
                        return true;
                    _second.Add(value, key);
                }
                return false;
            }
        }

        public override void SetKeyByValue(TValue value, TKey newKey)
        {
            lock (_syncRoot)
            {
                TKey oldKey;
                if (_second.TryGetValue(value, out oldKey))
                    _first.Remove(oldKey);

                TValue oldValue;
                if (_first.TryGetValue(newKey, out oldValue))
                    _second.Remove(oldValue);

                _second[value] = newKey;
                _first[newKey] = value;
            }
        }

        public override void SetValueByKey(TKey key, TValue newValue)
        {
            lock (_syncRoot)
            {
                TValue oldValue;
                if (_first.TryGetValue(key, out oldValue))
                    _second.Remove(oldValue);

                TKey oldKey;
                if (_second.TryGetValue(newValue, out oldKey))
                    _first.Remove(oldKey);

                _first[key] = newValue;
                _second[newValue] = key;
            }
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _first.GetEnumerator();
        }

        public static explicit operator OneToOneBidictionary<TValue, TKey>(OneToOneBidictionary<TKey, TValue> dict)
        {
            return dict.AsReverse();
        }
    }
}
