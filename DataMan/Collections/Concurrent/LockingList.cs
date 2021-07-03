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

namespace Baxendale.DataManagement.Collections.Concurrent
{
    public sealed class LockingList<T> : LockingListBase<T, List<T>>
    {
        public override List<T> Collection { get; }
        public override object SyncRoot { get; }

        public LockingList()
            : this((object)null)
        {
        }

        public LockingList(int capacity)
            : this(capacity, null)
        {
        }

        public LockingList(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public LockingList(object syncRoot)
        {
            Collection = new List<T>();
            SyncRoot = syncRoot ?? new object();
        }

        public LockingList(int capacity, object syncRoot)
        {
            Collection = new List<T>(capacity);
            SyncRoot = syncRoot ?? new object();
        }

        public LockingList(IEnumerable<T> collection, object syncRoot)
        {
            Collection = new List<T>(collection);
            SyncRoot = syncRoot ?? new object();
        }

        public override void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot) Collection.AddRange(collection);   
        }

        public LockingList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (SyncRoot) return new LockingList<TOutput>(Collection.ConvertAll(converter));
        }

        public override bool Exists(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.Exists(match);
        }

        public override T Find(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.Find(match);
        }

        public override List<T> FindAll(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.FindAll(match);
        }

        public override T FindLast(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.FindLast(match);
        }
        
        protected override int NolockFindLastIndex(Predicate<T> match)
        {
            return Collection.FindLastIndex(match);
        }

        public override void ForEach(Action<T> action)
        {
            lock (SyncRoot) Collection.ForEach(action);
        }

        public override List<T> GetRange(int index, int count)
        {
            lock (SyncRoot) return Collection.GetRange(index, count);
        }

        public override int IndexOf(T item)
        {
            lock (SyncRoot) return Collection.IndexOf(item);
        }

        public override int IndexOf(T item, int index)
        {
            lock (SyncRoot) return Collection.IndexOf(item, index);
        }

        public override int IndexOf(T item, int index, int count)
        {
            lock (SyncRoot) return Collection.IndexOf(item, index, count);
        }

        public override void Insert(int index, T item)
        {
            lock (SyncRoot) Collection.Insert(index, item);
        }

        public override void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (SyncRoot) Collection.InsertRange(index, collection);
        }

        public override int LastIndexOf(T item)
        {
            lock (SyncRoot) return Collection.LastIndexOf(item);
        }

        public override int LastIndexOf(T item, int index)
        {
            lock (SyncRoot) return Collection.LastIndexOf(item, index);
        }

        public override int LastIndexOf(T item, int index, int count)
        {
            lock (SyncRoot) return Collection.LastIndexOf(item, index, count);
        }

        public override int RemoveAll(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.RemoveAll(match);
        }

        public override void RemoveRange(int index, int count)
        {
            lock (SyncRoot) Collection.RemoveRange(index, count);
        }

        public override void Reverse()
        {
            lock (SyncRoot) Collection.Reverse();
        }

        public override void Reverse(int index, int count)
        {
            lock (SyncRoot) Collection.Reverse(index, count);
        }

        public override void Sort()
        {
            lock (SyncRoot) Collection.Sort();
        }

        public override void Sort(IComparer<T> comparer)
        {
            lock (SyncRoot) Collection.Sort(comparer);
        }

        public override void Sort(Comparison<T> comparison)
        {
            lock (SyncRoot) Collection.Sort(comparison);
        }

        public override void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (SyncRoot) Collection.Sort(index, count, comparer);
        }

        public override T[] ToArray()
        {
            lock (SyncRoot) return Collection.ToArray();
        }

        public void TrimExcess()
        {
            lock (SyncRoot) Collection.TrimExcess();
        }

        public override bool TrueForAll(Predicate<T> match)
        {
            lock (SyncRoot) return Collection.TrueForAll(match);
        }
    }
}
