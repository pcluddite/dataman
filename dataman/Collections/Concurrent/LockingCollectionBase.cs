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

namespace Baxendale.Data.Collections.Concurrent
{
    public abstract class LockingCollectionBase<T, CollectionType> : ILockingCollection<T>
        where CollectionType : ICollection<T>, new()
    {
        public abstract CollectionType Collection { get; }
        public abstract object SyncRoot { get; }

        public virtual bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public virtual int Count
        {
            get
            {
                lock (SyncRoot) return Collection.Count;
            }
        }

        public virtual void Add(T item)
        {
            lock (SyncRoot) Collection.Add(item);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                foreach (T item in collection)
                    Collection.Add(item);
            }
        }

        public virtual void Clear()
        {
            lock (SyncRoot) Collection.Clear();
        }

        public virtual bool Contains(T item)
        {
            lock (SyncRoot) return Collection.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot) Collection.CopyTo(array, arrayIndex);
        }

        public virtual bool Exists(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                foreach (T item in Collection)
                    if (match(item)) return true;
                return false;
            }
        }

        public virtual T Find(Predicate<T> match)
        {
            lock(SyncRoot)
            {
                foreach (T item in Collection)
                    if (match(item)) return item;
                return default(T);
            }
        }

        public virtual CollectionType FindAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                CollectionType collection = new CollectionType();
                foreach (T item in Collection)
                    if (match(item)) collection.Add(item);
                return collection;
            }
        }

        public virtual T FindLast(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                foreach (T item in Collection.Reverse())
                    if (match(item)) return item;
                return default(T);
            }
        }

        public virtual void ForEach(Action<T> action)
        {
            lock(SyncRoot)
            {
                foreach (T item in Collection)
                    action(item);
            }
        }

        public virtual bool Remove(T item)
        {
            lock (SyncRoot) return Collection.Remove(item);
        }

        public virtual int RemoveAll(Predicate<T> match)
        {
            lock(SyncRoot)
            {
                List<T> matchedList = new List<T>();
                foreach(T item in Collection)
                    if (match(item)) matchedList.Add(item);
                int matched = matchedList.Count;
                foreach (T item in matchedList)
                    Collection.Remove(item);
                return matched;
            }
        }

        public virtual LockingEnumerator<T> GetEnumerator()
        {
            return new LockingEnumerator<T>(this, SyncRoot);
        }

        #region ILockingCollection<T>

        ICollection<T> ILockingCollection<T>.Collection
        {
            get
            {
                return Collection;
            }
        }

        #endregion

        #region ICollection<T>

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                lock (SyncRoot) return Collection.IsReadOnly;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        #endregion

        #region IEnumerable<T>

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
