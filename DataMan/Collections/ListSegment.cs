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

namespace Baxendale.DataManagement.Collections
{
    public sealed class ListSegment<T> : IList<T>
    {
        private IList<T> _list;
        private int _startIndex;
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public T this[int index]
        {
            get
            {
                return _list[index + _startIndex];
            }
            set
            {
                _list[index + _startIndex] = value;
            }
        }

        public ListSegment(IList<T> list, int startIndex, int count)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if ((uint)startIndex >= (uint)list.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if ((uint)(startIndex + count) > (uint)list.Count) throw new ArgumentOutOfRangeException(nameof(count));
            _list = list;
            _startIndex = startIndex;
            _count = count;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int idx = 0; idx < _count; ++idx)
                array[arrayIndex + idx] = this[idx];
        }

        public int IndexOf(T item)
        {
            return IndexOf(item, 0, _count);
        }

        public int IndexOf(T item, int index)
        {
            return IndexOf(item, index, _count - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return IndexOf(item, index, count, null);
        }

        public int IndexOf(T item, int index, int count, IEqualityComparer<T> comparer)
        {
            if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            if ((uint)(index + count) > (uint)_count) throw new ArgumentOutOfRangeException(nameof(count));
            if (comparer == null) comparer = EqualityComparer<T>.Default;
            for (int idx = _startIndex + index, endIdx = idx + count; index < endIdx; ++index)
            {
                if (comparer.Equals(_list[index], item))
                    return index;
            }
            return -1;
        }

        #region IList<T>

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ICollection<T>

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            for (int idx = 0; idx < _count; ++idx)
                yield return this[idx];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
