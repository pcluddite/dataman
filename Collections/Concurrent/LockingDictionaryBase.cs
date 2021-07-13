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

namespace Baxendale.Data.Collections.Concurrent
{
    public abstract class LockingDictionaryBase<TKey, TValue, DictionaryType, KeyCollectionType, ValueCollectionType> : ILockingCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
        where DictionaryType : IDictionary<TKey, TValue>, new()
        where KeyCollectionType : ICollection<TKey>
        where ValueCollectionType : ICollection<TValue>
    {
        public abstract DictionaryType Dictionary { get; }
        public abstract object SyncRoot { get; }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                lock (SyncRoot) return Dictionary[key];
            }
            set
            {
                lock (SyncRoot) Dictionary[key] = value;
            }
        }

        public virtual int Count
        {
            get
            {
                lock (SyncRoot) return Dictionary.Count;
            }
        }

        public abstract KeyCollectionType Keys { get; }
        public abstract ValueCollectionType Value { get; }

        public virtual ICollection<TValue> Values
        {
            get
            {
                lock (SyncRoot) return new List<TValue>(Dictionary.Values);
            }
        }

        public virtual void Add(TKey key, TValue value)
        {
            lock (SyncRoot) Dictionary.Add(key, value);
        }

        public virtual bool ContainsKey(TKey key)
        {
            lock (SyncRoot) return Dictionary.ContainsKey(key);
        }

        public virtual bool Remove(TKey key)
        {
            lock (SyncRoot) return Dictionary.Remove(key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            lock(SyncRoot) return Dictionary.TryGetValue(key, out value);
        }

        public virtual void Clear()
        {
            lock (SyncRoot) Dictionary.Clear();
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (SyncRoot) Dictionary.CopyTo(array, arrayIndex);
        }

        public virtual LockingEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new LockingEnumerator<KeyValuePair<TKey, TValue>>(Dictionary, SyncRoot);
        }

        #region IDictionary<TKey, TValue>

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        #endregion

        #region ILockingCollection<KeyValuePair<TKey, TValue>>

        ICollection<KeyValuePair<TKey, TValue>> ILockingCollection<KeyValuePair<TKey, TValue>>.Collection
        {
            get
            {
                return Dictionary;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot) Dictionary.Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot) return Dictionary.Contains(item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((KeyValuePair<TKey, TValue>[])array, index);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (SyncRoot) return Dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>>

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public abstract class LockingDictionaryBase<TKey, TValue, DictionaryType> : LockingDictionaryBase<TKey, TValue, DictionaryType, LockingDictionaryBase<TKey, TValue, DictionaryType>.KeyCollection, LockingDictionaryBase<TKey, TValue, DictionaryType>.ValueCollection>
        where DictionaryType : IDictionary<TKey, TValue>, new()
    {
        public override KeyCollection Keys { get; }
        public override ValueCollection Value { get; }

        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private DictionaryType _dictionary;

            public object SyncRoot { get; }

            public bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public int Count
            {
                get
                {
                    lock (SyncRoot) return _dictionary.Count;
                }
            }

            public KeyCollection(LockingDictionaryBase<TKey, TValue, DictionaryType> dictionary)
            {
                if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
                _dictionary = dictionary.Dictionary;
                SyncRoot = dictionary.SyncRoot;
            }

            public bool Contains(TKey item)
            {
                lock (SyncRoot) return _dictionary.Keys.Contains(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                lock (SyncRoot) _dictionary.Keys.CopyTo(array, arrayIndex);
            }

            public LockingEnumerator<TKey> GetEnumerator()
            {
                return new LockingEnumerator<TKey>(_dictionary.Keys, SyncRoot);
            }

            #region ICollection<TKey>

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((TKey[])array, index);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<TKey>

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private DictionaryType _dictionary;

            public object SyncRoot { get; }

            public bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public int Count
            {
                get
                {
                    lock (SyncRoot) return _dictionary.Values.Count;
                }
            }

            public ValueCollection(LockingDictionaryBase<TKey, TValue, DictionaryType> dictionary)
            {
                if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
                _dictionary = dictionary.Dictionary;
                SyncRoot = dictionary.SyncRoot;
            }

            public bool Contains(TValue item)
            {
                lock (SyncRoot) return _dictionary.Values.Contains(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                lock (SyncRoot) _dictionary.Values.CopyTo(array, arrayIndex);
            }

            public LockingEnumerator<TValue> GetEnumerator()
            {
                return new LockingEnumerator<TValue>(_dictionary.Values, SyncRoot);
            }

            #region ICollection<TValue>

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((TValue[])array, index);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<TValue>

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
    }
}
