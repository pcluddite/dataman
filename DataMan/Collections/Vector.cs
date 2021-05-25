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

namespace Baxendale.DataManagement.Collections
{
    public class Vector<T> : IList<T>
    {
        private const int RESIZE_FACTOR = 2;
        private const int INITIAL_CAPACITY = 10;

        private T[] array;
        private int length;

        public T[] InternalArray
        {
            get
            {
                return array;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                if (value < array.Length)
                    Array.Clear(array, value, array.Length - value);
                length = value;
            }
        }

        public Vector()
        {
            array = new T[INITIAL_CAPACITY];
        }

        public Vector(int capacity)
        {
            array = new T[capacity];
        }

        public Vector(IList<T> items)
        {
            array = new T[items.Count];
            Length = array.Length;
            for (int idx = 0, len = Length; idx < len; ++idx)
                array[idx] = items[idx];
        }

        public Vector(IEnumerable<T> items)
        {
            array = items.ToArray();
            length = array.Length;
        }

        private Vector(T[] array)
        {
            this.array = array;
            length = array.Length;
        }

        public void Fill(T item, int startIndex, int count)
        {
            for (int index = 0; index < count; ++index)
                this[index + startIndex] = item;
        }

        #region IList<int> Members

        public int IndexOf(T item)
        {
            int idx = Array.IndexOf(array, item, 0, Math.Min(length, array.Length));
            if (idx < 0 && array.Length < length && object.Equals(item, default(T)))
                return array.Length;
            return idx;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException();
                if (index > array.Length)
                    return default(T);
                return array[index];
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                EnsureCapacity(index);
                array[index] = value;
            }
        }

        private void EnsureCapacity(int index)
        {
            if (index >= array.Length)
            {
                Array.Resize(ref array, index * RESIZE_FACTOR);
                length = index + 1;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        public void TrimExcess()
        {
            if (length < array.Length)
                Array.Resize(ref array, length);
        }

        #endregion

        #region ICollection<int> Members

        public void Add(T item)
        {
            this[length++] = item;
        }

        public void Clear()
        {
            Array.Clear(array, 0, Length);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] dest, int destIndex)
        {
            int len = Length;
            int count = Math.Min(array.Length, len);
            Array.Copy(array, 0, dest, destIndex, count);
            if (count < len)
                Array.Clear(array, count, len - count);
        }

        int ICollection<T>.Count
        {
            get { return Length; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return i < array.Length ? array[i] : default(T);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public static implicit operator Vector<T>(T[] array)
        {
            if (array == null)
                return new Vector<T>();
            return new Vector<T>(array);
        }
    }
}
