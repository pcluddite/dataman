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

namespace Baxendale.DataManagement.Collections.Concurrent
{
    public abstract class LockingSetBase<T, SetType> : ILockingCollection<T>, ISet<T>
        where SetType : ISet<T>, new()
    {
        public abstract SetType Set { get; }
        public abstract object SyncRoot { get; }

        public virtual int Count
        {
            get
            {
                lock (SyncRoot) return Set.Count;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public virtual bool Add(T item)
        {
            lock (SyncRoot) return Set.Add(item);
        }

        public virtual void Clear()
        {
            lock (SyncRoot) Set.Clear();
        }

        public virtual bool Contains(T item)
        {
            lock (SyncRoot) return Set.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot) Set.CopyTo(array, arrayIndex);
        }

        public virtual void ExceptWith(IEnumerable<T> other)
        {
            lock (SyncRoot) Set.ExceptWith(other);
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            lock (SyncRoot) Set.IntersectWith(other);
        }

        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.IsProperSubsetOf(other);
        }

        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.IsProperSupersetOf(other);
        }

        public virtual bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.IsSubsetOf(other);
        }

        public virtual bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.IsSupersetOf(other);
        }

        public virtual bool Overlaps(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.Overlaps(other);
        }

        public virtual bool Remove(T item)
        {
            lock (SyncRoot) return Set.Remove(item);
        }

        public virtual bool SetEquals(IEnumerable<T> other)
        {
            lock (SyncRoot) return Set.SetEquals(other);
        }

        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (SyncRoot) Set.SymmetricExceptWith(other);
        }

        public virtual void UnionWith(IEnumerable<T> other)
        {
            lock (SyncRoot) Set.UnionWith(other);
        }

        public virtual LockingEnumerator<T> GetEnumerator()
        {
            return new LockingEnumerator<T>(Set, SyncRoot);
        }

        #region ILockingCollection<T>

        ICollection<T> ILockingCollection<T>.Collection
        {
            get
            {
                return Set;
            }
        }

        #endregion

        #region ICollection<T>

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void ICollection<T>.Add(T item)
        {
            lock (SyncRoot) ((ICollection<T>)Set).Add(item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        #endregion

        #region IEnumerable<T>

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
