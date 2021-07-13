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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Baxendale.Data.Collections.Concurrent
{
    public sealed class LockingDictionary<TKey, TValue> : LockingDictionaryBase<TKey, TValue, Dictionary<TKey, TValue>>, ISerializable
    {
        public override Dictionary<TKey, TValue> Dictionary { get; }
        public override object SyncRoot { get; }

        public LockingDictionary()
            : this((object)null)
        {
        }

        public LockingDictionary(int capacity)
            : this(capacity, null)
        {
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        public LockingDictionary(IEqualityComparer<TKey> comparer)
            : this(comparer, null)
        {
        }

        public LockingDictionary(IEqualityComparer<TKey> comparer, object syncRoot)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingDictionary(object syncRoot)
{
            Dictionary = new Dictionary<TKey, TValue>();
            SyncRoot = syncRoot ?? new object();
        }

        public LockingDictionary(int capacity, object syncRoot)
            : this(capacity, EqualityComparer<TKey>.Default, syncRoot)
        {
        }

        public LockingDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : this(capacity, comparer, null)
        {
        }

        public LockingDictionary(int capacity, IEqualityComparer<TKey> comparer, object syncRoot)
        {
            Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, object syncRoot)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(dictionary, comparer, null)
        {
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer, object syncRoot)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            SyncRoot = syncRoot ?? new object();
        }

        public bool ContainsValue(TValue value)
        {
            return Dictionary.ContainsValue(value);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (SyncRoot) Dictionary.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            lock (SyncRoot) Dictionary.OnDeserialization(sender);
        }
    }
}
