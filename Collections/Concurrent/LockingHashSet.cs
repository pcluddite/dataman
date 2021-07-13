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
using System.Runtime.Serialization;

namespace Baxendale.Data.Collections.Concurrent
{
    public sealed class LockingHashSet<T> : LockingSetBase<T, HashSet<T>>, ISerializable
    {
        public override HashSet<T> Set { get; }
        public override object SyncRoot { get; }

        public LockingHashSet()
            : this((object)null)
        {
        }

        public LockingHashSet(object syncRoot)
        {
            Set = new HashSet<T>();
            SyncRoot = syncRoot ?? new object();
        }

        public LockingHashSet(IEnumerable<T> collection)
            : this(collection, (object)null)
        {
        }

        public LockingHashSet(IEnumerable<T> collection, object syncRoot)
        {
            Set = new HashSet<T>(collection);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingHashSet(IEqualityComparer<T> comparer)
            : this(comparer, null)
        {
        }

        public LockingHashSet(IEqualityComparer<T> comparer, object syncRoot)
        {
            Set = new HashSet<T>(comparer);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(collection, comparer, null)
        {
        }

        public LockingHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer, object syncRoot)
        {
            Set = new HashSet<T>(collection, comparer);
            SyncRoot = syncRoot ?? new object();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (SyncRoot) Set.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            lock (SyncRoot) Set.OnDeserialization(sender);
        }

        public void TrimExcess()
        {
            lock (SyncRoot) Set.TrimExcess();
        }
    }
}
