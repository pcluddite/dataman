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
    internal class Sublist<T> : IList<T>
    {
        private IList<T> _list;
        private int _startIndex;
        private int _count;

        public Sublist(IList<T> list, int startIndex, int count)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if ((uint)startIndex >= (uint)list.Count) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if ((uint)(startIndex + count) > (uint)list.Count) throw new ArgumentOutOfRangeException(nameof(count));
            _list = list;
            _startIndex = startIndex;
            _count = count;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            for (int idx = 0; idx < _count; ++idx)
            {
                if (Equals(_list[idx], item))
                    return idx;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
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

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
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

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int idx = 0; idx < _count; ++idx)
                yield return this[idx];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
