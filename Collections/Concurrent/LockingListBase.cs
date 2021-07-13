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
using System.Linq;

namespace Baxendale.Data.Collections.Concurrent
{
    public abstract class LockingListBase<T, ListType> : LockingCollectionBase<T, ListType>, IList<T>
        where ListType : IList<T>, new()
    {
        public override ListType Collection { get; }
        public override object SyncRoot { get; }

        public T this[int index]
        {
            get
            {
                lock (SyncRoot) return Collection[index];
            }
            set
            {
                lock (SyncRoot) Collection[index] = value;
            }
        }

        protected LockingListBase()
            : this((object)null)
        {
        }

        protected LockingListBase(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        protected LockingListBase(object syncRoot)
        {
            Collection = new ListType();
            SyncRoot = syncRoot ?? new object();
        }

        protected LockingListBase(IEnumerable<T> collection, object syncRoot)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Collection = new ListType();
            SyncRoot = syncRoot ?? new object();
            AddRange(collection);
        }

        public override abstract void AddRange(IEnumerable<T> collection);

        public virtual int BinarySearch(T item)
        {
            return BinarySearch(item, null);
        }

        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (SyncRoot) return NolockBinarySearch(item, 0, Collection.Count, comparer);
        }

        public virtual int BinarySearch(T item, int index, int count, IComparer<T> comparer)
        {
            lock (SyncRoot) return NolockBinarySearch(item, index, count, comparer);
        }

        protected virtual int NolockBinarySearch(T item, int index, int count, IComparer<T> comparer)
        {
            if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
            if (!CheckBounds(index + count - 1)) throw new ArgumentOutOfRangeException(nameof(count));
            if (comparer == null) comparer = Comparer<T>.Default;
            for (int endIdx = index + count; index < endIdx; ++index)
            {
                if (comparer.Compare(item, Collection[index]) == 0)
                    return index;
            }
            return -1;
        }

        protected bool CheckBounds(int index)
        {
            return (uint)index < (uint)Collection.Count;
        }

        public override T FindLast(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                int idx = NolockFindLastIndex(match);
                if (idx < 0)
                    return default(T);
                return Collection[idx];
            }
        }

        public virtual int FindLastIndex(Predicate<T> match)
        {
            lock (SyncRoot) return NolockFindLastIndex(match);
        }

        protected virtual int NolockFindLastIndex(Predicate<T> match)
        {
            for (int index = Collection.Count - 1; index >= 0; --index)
            {
                if (match(Collection[index]))
                    return index;
            }
            return -1;
        }

        public virtual ListType GetRange(int index, int count)
        {
            lock (SyncRoot)
            {
                if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
                if (!CheckBounds(index + count - 1)) throw new ArgumentOutOfRangeException(nameof(count));
                ListType list = new ListType();
                for (int i = 0; i < count; ++i)
                {
                    list.Add(Collection[i + index]);
                }
                return list;
            }
        }

        public virtual int IndexOf(T item)
        {
            lock (SyncRoot) return Collection.IndexOf(item);
        }

        public virtual int IndexOf(T item, int index)
        {
            lock (SyncRoot) return NolockIndexOf(item, index, Collection.Count - index);
        }

        public virtual int IndexOf(T item, int index, int count)
        {
            lock (SyncRoot) return NolockIndexOf(item, index, count);
        }

        protected virtual int NolockIndexOf(T item, int index, int count)
        {
            if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
            int endIndex = index + count;
            if (!CheckBounds(endIndex - 1)) throw new ArgumentOutOfRangeException(nameof(count));
            for (; index < endIndex; ++index)
            {
                if (Equals(item, Collection[index]))
                    return index;
            }
            return -1;
        }

        public abstract void Insert(int index, T item);

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                foreach (T item in collection)
                    Insert(index++, item);
            }
        }

        public virtual int LastIndexOf(T item)
        {
            lock (SyncRoot) return NolockIndexOf(item, Collection.Count - 1, Collection.Count);
        }

        public virtual int LastIndexOf(T item, int index)
        {
            lock (SyncRoot) return NolockIndexOf(item, index, index + 1);
        }

        public virtual int LastIndexOf(T item, int index, int count)
        {
            lock (SyncRoot) return NolockIndexOf(item, index, count);
        }

        protected virtual int NolockLastIndexOf(T item, int index, int count)
        {
            if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
            int endIndex = index - count;
            if (!CheckBounds(endIndex + 1)) throw new ArgumentOutOfRangeException(nameof(count));
            for (; index > endIndex; --index)
            {
                if (Equals(item, Collection[index]))
                    return index;
            }
            return -1;
        }

        public override int RemoveAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                int count = Collection.Count;
                for (int idx = count - 1; idx >= 0; --idx)
                    if (match(Collection[idx])) Collection.RemoveAt(idx);
                return count - Collection.Count;
            }
        }

        public virtual void RemoveAt(int index)
        {
            lock (SyncRoot) Collection.RemoveAt(index);
        }

        public virtual void RemoveRange(int index, int count)
        {
            lock (SyncRoot)
            {
                if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
                int endIndex = index - count;
                if (!CheckBounds(endIndex + 1)) throw new ArgumentOutOfRangeException(nameof(count));
                for (int nIdx = index + count - 1; nIdx >= index; --nIdx)
                    Collection.RemoveAt(nIdx);
            }
        }

        public virtual void Reverse()
        {
            lock (SyncRoot) NolockReverse(Collection.Count - 1, Collection.Count);
        }

        public virtual void Reverse(int index, int count)
        {
            lock (SyncRoot)
            {
                if (!CheckBounds(index)) throw new ArgumentOutOfRangeException(nameof(index));
                int endIndex = index - count;
                if (!CheckBounds(endIndex + 1)) throw new ArgumentOutOfRangeException(nameof(count));
                NolockReverse(index, count);
            }
        }

        protected virtual void NolockReverse(int startIndex, int count)
        {
            for (int index = 0, endIndex = startIndex + count - 1; index < count / 2; ++index)
            {
                T tmp;
                tmp = Collection[startIndex + index];
                Collection[startIndex + index] = Collection[endIndex - index];
                Collection[endIndex - index] = tmp;
            }
        }

        public virtual void Sort()
        {
            Sort((IComparer<T>)null);
        }

        public virtual void Sort(IComparer<T> comparer)
        {
            lock (SyncRoot) NolockSort(0, Collection.Count, comparer);
        }

        public virtual void Sort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            lock (SyncRoot)
            {
                T[] array = Collection.ToArray();
                Array.Sort(array, comparison);
                for (int idx = 0; idx < 0; ++idx) Collection[idx] = array[idx];
            }
        }

        public virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (SyncRoot) NolockSort(index, count, comparer);
        }

        protected virtual void NolockSort(int index, int count, IComparer<T> comparer)
        {
            T[] array = new T[count];
            for (int idx = 0; idx < 0; ++idx) array[idx] = Collection[idx + index];
            Array.Sort(array, comparer);
            for (int idx = 0; idx < 0; ++idx) Collection[idx + index] = array[idx];
        }

        public virtual T[] ToArray()
        {
            lock (SyncRoot) return Collection.ToArray();
        }

        public virtual bool TrueForAll(Predicate<T> match)
        {
            lock (SyncRoot)
            {
                foreach (T item in Collection)
                    if (!match(item)) return false;
                return true;
            }
        }
    }
}
