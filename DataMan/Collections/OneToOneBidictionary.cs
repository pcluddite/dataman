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

namespace Baxendale.DataManagement.Collections
{
    public class OneToOneBidictionary<TKey, TValue> : BidirectionalDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _first;
        private Dictionary<TValue, TKey> _second;

        protected override IDictionary<TKey, TValue> KeyValueDictionary
        {
            get
            {
                return _first;
            }
        }

        protected override IDictionary<TValue, TKey> ValueKeyDictionary
        {
            get
            {
                return _second;
            }
        }

        public override int Count
        {
            get
            {
                return _first.Count;
            }
        }

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

        public OneToOneBidictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null, null)
        {
        }

        public OneToOneBidictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new Dictionary<TKey, TValue>(keyComparer);
            _second = new Dictionary<TValue, TKey>(valueComparer);
            foreach (KeyValuePair<TKey, TValue> kv in dictionary)
                Add(kv.Key, kv.Value);
        }

        public OneToOneBidictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _first = new Dictionary<TKey, TValue>(capacity, keyComparer);
            _second = new Dictionary<TValue, TKey>(capacity, valueComparer);
        }

        private OneToOneBidictionary(Dictionary<TKey, TValue> first, Dictionary<TValue, TKey> second)
        {
            _first = first;
            _second = second;
        }

        public override void Add(TKey key, TValue value)
        {
            KeyValueDictionary.Add(key, value);
            ValueKeyDictionary.Add(value, key);
        }


        public override void Clear()
        {
            _first.Clear();
            _second.Clear();
        }

        public override TValue GetValueByKey(TKey key)
        {
            return KeyValueDictionary[key];
        }

        public override TKey GetKeyByValue(TValue value)
        {
            return ValueKeyDictionary[value];
        }

        public override bool RemoveByKey(TKey key)
        {
            TValue value;
            if (!KeyValueDictionary.TryGetValue(key, out value))
                return false;
            if (KeyValueDictionary.Remove(key))
            {
                if (ValueKeyDictionary.Remove(value))
                    return true;
                KeyValueDictionary.Add(key, value);
            }
            return false;
        }

        public override bool RemoveByValue(TValue value)
        {
            TKey key;
            if (!ValueKeyDictionary.TryGetValue(value, out key))
                return false;
            if (ValueKeyDictionary.Remove(value))
            {
                if (KeyValueDictionary.Remove(key))
                    return true;
                ValueKeyDictionary.Add(value, key);
            }
            return false;
        }

        public override void SetKeyByValue(TValue value, TKey newKey)
        {
            TKey oldKey;
            if (!ValueKeyDictionary.TryGetValue(value, out oldKey))
                throw new KeyNotFoundException();
            KeyValueDictionary.Remove(oldKey);

            TValue oldValue;
            if (KeyValueDictionary.TryGetValue(newKey, out oldValue))
                ValueKeyDictionary.Remove(oldValue);

            ValueKeyDictionary[value] = newKey;
            KeyValueDictionary[newKey] = value;
        }

        public override void SetValueByKey(TKey key, TValue newValue)
        {
            TValue oldValue;
            if (!KeyValueDictionary.TryGetValue(key, out oldValue))
                throw new KeyNotFoundException();
            ValueKeyDictionary.Remove(oldValue);

            TKey oldKey;
            if (ValueKeyDictionary.TryGetValue(newValue, out oldKey))
                KeyValueDictionary.Remove(oldKey);

            KeyValueDictionary[key] = newValue;
            ValueKeyDictionary[newValue] = key;
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _first.GetEnumerator();
        }

        public static implicit operator OneToOneBidictionary<TValue, TKey>(OneToOneBidictionary<TKey, TValue> o)
        {
            return new OneToOneBidictionary<TValue, TKey>(o._second, o._first);
        }
    }
}
