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
using Baxendale.DataManagement.Collections.ReadOnly;

namespace Baxendale.DataManagement.Collections
{
    public class CompositeComparer<T> : IComparer<T>, ICollection<IComparer<T>>
    {
        private readonly List<IComparer<T>> _comparers;

        public virtual IEnumerable<IComparer<T>> Comparers
        {
            get
            {
                return new ReadOnlyList<IComparer<T>>(_comparers);
            }
        }

        public virtual int Count
        {
            get
            {
                return _comparers.Count;
            }
        }

        public CompositeComparer()
        {
            _comparers = new List<IComparer<T>> { Comparer<T>.Default };
        }

        public CompositeComparer(IEnumerable<IComparer<T>> comparers)
        {
            _comparers = new List<IComparer<T>>(comparers);
        }

        public virtual void Add(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            _comparers.Add(comparer);
        }

        public virtual void Add<V>(IComparer<V> comparer, Converter<T, V> converter)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            _comparers.Add(new CompositeComparerElement<V>(comparer, converter));
        }

        public virtual void Add(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            _comparers.Add(Comparer<T>.Create(comparison));
        }

        public virtual void Add<V>(Comparison<V> comparison, Converter<T, V> converter)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            _comparers.Add(new CompositeComparerElement<V>(comparison, converter));
        }

        public virtual bool Remove(IComparer<T> comparer)
        {
            return _comparers.Remove(comparer);
        }

        public virtual bool Remove<V>(IComparer<V> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            for (int idx = 0; idx < _comparers.Count; ++idx)
            {
                CompositeComparerElement<V> other = _comparers[idx] as CompositeComparerElement<V>;
                if (other?.Comparer == comparer)
                {
                    _comparers.RemoveAt(idx);
                    return true;
                }
            }
            return false;
        }

        public virtual int Compare(T x, T y)
        {
            int? result = null;
            foreach (IComparer<T> comparer in Comparers)
            {
                result = comparer.Compare(x, y);
                if (result != 0)
                    return result.Value;
            }
            if (result == null)
                throw new InvalidOperationException("CompositeComparer must have at least one sub comparer");
            return result.Value;
        }

        public virtual void TrimExcess()
        {
            _comparers.TrimExcess();
        }

        public virtual void Clear()
        {
            _comparers.Clear();
        }

        public virtual bool Contains(IComparer<T> item)
        {
            return _comparers.Contains(item);
        }

        public virtual bool Contains<V>(IComparer<V> comparer)
        {
            foreach(CompositeComparerElement<V> other in _comparers.OfType<CompositeComparerElement<V>>())
            {
                if (other.Comparer == comparer)
                    return true;
            }
            return false;
        }

        public virtual void CopyTo(IComparer<T>[] array, int arrayIndex)
        {
            _comparers.CopyTo(array, arrayIndex);
        }

        #region ICollection<IComparer<T>>

        bool ICollection<IComparer<T>>.IsReadOnly => false;

        #endregion

        #region IEnumerable<IComparer<T>>

        public IEnumerator<IComparer<T>> GetEnumerator()
        {
            return _comparers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_comparers).GetEnumerator();
        }

        #endregion

        private class CompositeComparerElement<V> : IComparer<T>
        {
            public Converter<T, V> Converter { get; }
            public IComparer<V> Comparer { get; }

            public CompositeComparerElement(Comparison<V> comparison, Converter<T, V> converter)
                : this(Comparer<V>.Create(comparison), converter)
            {
            }

            public CompositeComparerElement(IComparer<V> comparer, Converter<T, V> converter)
            {
                Converter = converter;
                Comparer = comparer;
            }

            public int Compare(V x, V y)
            {
                return Comparer.Compare(x, y);
            }

            public int Compare(T x, T y)
            {
                return Compare(Converter(x), Converter(y));
            }
        }
    }
}
