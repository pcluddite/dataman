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
    public class ReverseEnumerator<T> : IEnumerator<T>
    {
        private readonly IList<T> _list;
        private readonly int _startIndex;
        private int _index;
        private int _count;

        public T Current
        {
            get
            {
                return _list[_index];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public ReverseEnumerator(T[] array)
            : this(array, array.Length - 1, array.Length)
        {
        }

        public ReverseEnumerator(T[] array, int index)
            : this(array, index, index + 1)
        {
        }

        public ReverseEnumerator(T[] array, int index, int count)
            : this((IList<T>)array, index, count)
        {
        }

        public ReverseEnumerator(IList<T> list)
            : this(list, list.Count - 1, list.Count)
        {
        }

        public ReverseEnumerator(IList<T> list, int index)
            : this(list, index, index + 1)
        {
        }

        public ReverseEnumerator(IList<T> list, int index, int count)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index - count + 1 < 0) throw new ArgumentOutOfRangeException(nameof(count));
            _count = count;
            _list = list;
            _startIndex = index;
            _index = _startIndex;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return _startIndex - _index-- < _count;
        }

        public void Reset()
        {
            _index = _startIndex;
        }
    }
}
