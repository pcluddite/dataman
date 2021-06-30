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
    public static class Collections<T>
    {
        public static readonly IList<T> EmptyList = new List<T>().AsReadOnly();
        public static readonly ICollection<T> EmptyCollection = EmptyList;

        public static ISet<T> SingletonSet(T value)
        {
            return new SingletonCollection(value);
        }

        public static ISet<T> SingletonSet(T value, IEqualityComparer<T> comparer)
        {
            return new SingletonCollection(value, comparer);
        }

        private class SingletonCollection : ISet<T>
        {
            public T Value { get; }
            public IEqualityComparer<T> Comparer { get; }

            int ICollection<T>.Count => 1;
            bool ICollection<T>.IsReadOnly => true;

            public SingletonCollection(T value)
                : this(value, null)
            {
            }

            public SingletonCollection(T value, IEqualityComparer<T> comparer)
            {
                Value = value;
                Comparer = comparer ?? EqualityComparer<T>.Default;
            }

            bool ISet<T>.Add(T item)
            {
                return false;
            }

            void ISet<T>.UnionWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            void ISet<T>.IntersectWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            void ISet<T>.ExceptWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.Overlaps(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            bool ISet<T>.SetEquals(IEnumerable<T> other)
            {
                int i = 0;
                foreach(T item in other)
                {
                    if (i > 0 || !Comparer.Equals(Value, item))
                        return false;
                    ++i;
                }
                return true;
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotSupportedException();
            }

            void ICollection<T>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<T>.Contains(T item)
            {
                return Comparer.Equals(item, Value);
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if ((uint)arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                array[arrayIndex] = Value;
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return Value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
