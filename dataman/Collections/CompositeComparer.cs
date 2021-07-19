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
using Baxendale.Data.Collections.ReadOnly;

namespace Baxendale.Data.Collections
{
    public class CompositeComparer<T> : IComparer<T>, IList<IComparer<T>>, ICollection<IComparer<T>>
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

        public virtual IComparer<T> this[int index]
        {
            get
            {
                return _comparers[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _comparers[index] = value;
            }
        }

        public CompositeComparer()
        {
            _comparers = new List<IComparer<T>> { Comparer<T>.Default };
        }

        public CompositeComparer(IEnumerable<IComparer<T>> comparers)
        {
            if (comparers == null) throw new ArgumentNullException(nameof(comparers));
            _comparers = new List<IComparer<T>>(comparers);
        }

        public virtual void Add(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            _comparers.Add(comparer);
        }

        public virtual IComparer<T> Add(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            IComparer<T> comparer = Comparer<T>.Create(comparison);
            _comparers.Add(comparer);
            return comparer;
        }

        public virtual IComparer<T> Add<V>(IComparer<V> comparer, Converter<T, V> converter)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            IComparer<T> converted = new ComponentComparer<V>(comparer, converter);
            _comparers.Add(converted);
            return converted;
        }

        public virtual IComparer<T> Add<V>(Comparison<V> comparison, Converter<T, V> converter)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            IComparer<T> converted = new ComponentComparer<V>(comparison, converter);
            _comparers.Add(converted);
            return converted;
        }

        public virtual int IndexOf(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            return _comparers.IndexOf(comparer);
        }

        public virtual int IndexOf<V>(IComparer<V> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            for (int idx = 0; idx < _comparers.Count; ++idx)
            {
                ComponentComparer<V> other = _comparers[idx] as ComponentComparer<V>;
                if (other?.Comparer == comparer)
                    return idx;
            }
            return -1;
        }

        public virtual void Insert(int index, IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            _comparers.Insert(index, comparer);
        }

        public virtual void Insert<V>(int index, Comparison<V> comparison, Converter<T, V> converter)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            _comparers.Insert(index, new ComponentComparer<V>(comparison, converter));
        }

        public virtual void RemoveAt(int index)
        {
            if (Count == 1) throw new TooFewComparersException();
            _comparers.RemoveAt(index);
        }

        public virtual bool Remove(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (Count == 1) throw new TooFewComparersException();
            return _comparers.Remove(comparer);
        }

        public virtual bool Remove<V>(IComparer<V> comparer)
        {
            int idx = IndexOf(comparer); // null check is performed in IndexOf
            if (idx == -1)
                return false;
            if (Count == 1) throw new TooFewComparersException();
            _comparers.RemoveAt(idx);
            return true;
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
                throw new TooFewComparersException();
            return result.Value;
        }

        public virtual void TrimExcess()
        {
            _comparers.TrimExcess();
        }

        public virtual void Clear()
        {
            _comparers.Clear();
            _comparers.Add(Comparer<T>.Default);
        }

        public virtual bool Contains(IComparer<T> item)
        {
            return _comparers.Contains(item);
        }

        public virtual bool Contains<V>(IComparer<V> comparer)
        {
            return IndexOf(comparer) > -1;
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
            return GetEnumerator();
        }

        #endregion

        private class ComponentComparer<V> : IComparer<T>
        {
            public Converter<T, V> Converter { get; }
            public IComparer<V> Comparer { get; }

            public ComponentComparer(Comparison<V> comparison, Converter<T, V> converter)
                : this(Comparer<V>.Create(comparison), converter)
            {
            }

            public ComponentComparer(IComparer<V> comparer, Converter<T, V> converter)
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

        private class TooFewComparersException : InvalidOperationException
        {
            public TooFewComparersException()
                : base("CompositeComparer must have at least one component comparer")
            {
            }
        }
    }
}
