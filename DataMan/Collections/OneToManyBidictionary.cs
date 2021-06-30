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
using System.Collections.Generic;
using System.Linq;

namespace Baxendale.DataManagement.Collections
{
    public sealed class OneToManyBidictionary<TKey, TValue> : BidirectionalDictionary<TKey, TValue, OneToManyBidictionary<TValue, TKey>>
    {
        private MultiValueDictionary<TKey, TValue> _first;
        private MultiValueDictionary<TValue, TKey> _second;

        private OneToManyBidictionary<TValue, TKey> _reverse;

        protected override IDictionary<TKey, TValue> KeyValueDictionary => _first;
        protected override IDictionary<TValue, TKey> ValueKeyDictionary => _second;

        public override int Count => _first.Count;

        public OneToManyBidictionary()
            : this(0, null, null)
        {
        }

        public OneToManyBidictionary(int capacity)
            : this(capacity, null, null)
        {
        }

        public OneToManyBidictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : this(0, keyComparer, valueComparer)
        {
        }

        public OneToManyBidictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null, null)
        {
        }

        public OneToManyBidictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new MultiValueDictionary<TKey, TValue>(keyComparer, valueComparer);
            _second = new MultiValueDictionary<TValue, TKey>(valueComparer, keyComparer);
            foreach (KeyValuePair<TKey, TValue> kv in dictionary)
                Add(kv.Key, kv.Value);
        }

        public OneToManyBidictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new MultiValueDictionary<TKey, TValue>(capacity, keyComparer, valueComparer);
            _second = new MultiValueDictionary<TValue, TKey>(capacity, valueComparer, keyComparer);
        }

        private OneToManyBidictionary(MultiValueDictionary<TKey, TValue> first, MultiValueDictionary<TValue, TKey> second)
        {
            _first = first;
            _second = second;
        }

        public override void Add(TKey key, TValue value)
        {
            _first.Add(key, value);
            _second.Add(value, key);
        }

        public override OneToManyBidictionary<TValue, TKey> AsReverse()
        {
            if (_reverse == null)
                _reverse = new OneToManyBidictionary<TValue, TKey>(_second, _first);
            return _reverse;
        }

        public override void Clear()
        {
            _first.Clear();
            _second.Clear();
        }

        public override TKey GetKeyByValue(TValue value)
        {
            return GetKeysByValue(value).First();
        }

        public IEnumerable<TKey> GetKeysByValue(TValue value)
        {
            return _second[value];
        }

        public override TValue GetValueByKey(TKey key)
        {
            return GetValuesByKey(key).First();
        }

        public IEnumerable<TValue> GetValuesByKey(TKey key)
        {
            return _first[key];
        }

        public override bool RemoveByKey(TKey key)
        {
            IEnumerable<TValue> values;
            if (_first.TryGetValue(key, out values))
            {
                foreach(TValue value in values)
                    _second.Remove(value, key);
                _first.Remove(key);
                return true;
            }
            return false;
        }

        public override bool RemoveByValue(TValue value)
        {
            IEnumerable<TKey> keys;
            if (_second.TryGetValue(value, out keys))
            {
                foreach (TKey key in keys)
                    _first.Remove(key, value);
                _second.Remove(value);
                return true;
            }
            return false;
        }

        public override void SetKeyByValue(TValue value, TKey newKey)
        {
            Add(newKey, value);
        }

        public override void SetValueByKey(TKey key, TValue newValue)
        {
            Add(key, newValue);
        }

        public override bool TryGetKey(TValue value, out TKey key)
        {
            IEnumerable<TKey> keys;
            if (TryGetKeys(value, out keys))
            {
                key = keys.First();
                return true;
            }
            key = default(TKey);
            return false;
        }

        public bool TryGetKeys(TValue value, out IEnumerable<TKey> keys)
        {
            return _second.TryGetValue(value, out keys);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            IEnumerable<TValue> values;
            if (TryGetValues(key, out values))
            {
                value = values.First();
                return true;
            }
            value = default(TValue);
            return false;
        }

        public bool TryGetValues(TKey key, out IEnumerable<TValue> values)
        {
            return _first.TryGetValue(key, out values);
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _first.GetEnumerator();
        }

        public static explicit operator OneToManyBidictionary<TValue, TKey>(OneToManyBidictionary<TKey, TValue> dict)
        {
            return dict.AsReverse();
        }
    }
}
