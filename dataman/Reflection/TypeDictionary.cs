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

namespace Baxendale.Data.Reflection
{
    public class TypeDictionary<TValue> : ICollection<KeyValuePair<Type, TValue>>, IDictionary<Type, TValue>
    {
        private readonly Dictionary<Type, TValue> _dictionary;

        public int Count => _dictionary.Count;

        public ICollection<Type> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public TValue this[Type key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException();
                return value;
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        public TypeDictionary()
        {
            _dictionary = new Dictionary<Type, TValue>();
        }

        public TypeDictionary(IEqualityComparer<Type> comparer)
        {
            _dictionary = new Dictionary<Type, TValue>(comparer);
        }

        public TypeDictionary(IDictionary<Type, TValue> dictionary)
        {
            _dictionary = new Dictionary<Type, TValue>(dictionary);
        }

        public TypeDictionary(int capacity)
        {
            _dictionary = new Dictionary<Type, TValue>(capacity);
        }

        public TypeDictionary(int capacity, IEqualityComparer<Type> comparer)
        {
            _dictionary = new Dictionary<Type, TValue>(capacity, comparer);
        }

        public TypeDictionary(IDictionary<Type, TValue> dictionary, IEqualityComparer<Type> comparer)
        {
            _dictionary = new Dictionary<Type, TValue>(dictionary, comparer);
        }

        public void Add(Type key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool ContainsKey(Type key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(Type key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(Type key, out TValue value)
        {
            Dictionary<Type, ISet<Type>> interfaces = null;

            foreach (Type type in key.GetHierarchy())
            {
                if (_dictionary.TryGetValue(type, out value))
                    return true;

                if (interfaces == null)
                    interfaces = type.GetDeclaredInterfaces();

                foreach (Type i in interfaces[type])
                {
                    if (_dictionary.TryGetValue(i, out value))
                        return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public Type FindBestMatch(Type key)
        {
            Dictionary<Type, ISet<Type>> interfaces = null;

            foreach (Type type in key.GetHierarchy())
            {
                if (_dictionary.ContainsKey(type))
                    return type;

                if (interfaces == null)
                    interfaces = type.GetDeclaredInterfaces();

                foreach (Type i in interfaces[type])
                {
                    if (_dictionary.ContainsKey(i))
                        return i;
                }
            }

            return null;
        }

        #region ICollection<KeyValuePair<TKey, TValue>>

        bool ICollection<KeyValuePair<Type, TValue>>.IsReadOnly { get; }

        void ICollection<KeyValuePair<Type, TValue>>.Add(KeyValuePair<Type, TValue> item)
        {
            ((ICollection<KeyValuePair<Type, TValue>>)_dictionary).Add(item);
        }

        bool ICollection<KeyValuePair<Type, TValue>>.Contains(KeyValuePair<Type, TValue> item)
        {
            return ((ICollection<KeyValuePair<Type, TValue>>)_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<Type, TValue>>.CopyTo(KeyValuePair<Type, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Type, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<Type, TValue>>.Remove(KeyValuePair<Type, TValue> item)
        {
            return ((ICollection<KeyValuePair<Type, TValue>>)_dictionary).Remove(item);
        }

        #endregion

        public IEnumerator<KeyValuePair<Type, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
