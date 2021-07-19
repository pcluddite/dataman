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
using System.Text;
using Baxendale.Data.Collections.ReadOnly;

namespace Baxendale.Data.Collections
{
    public static class Collections
    {
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> collection)
        {
            return new ReadOnlyCollection<T>(collection);
        }

        public static IEnumerable<KeyValuePair<TValue, TKey>> SwapKeyValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            foreach (KeyValuePair<TKey, TValue> kv in collection)
                yield return new KeyValuePair<TValue, TKey>(kv.Value, kv.Key);
        }

        public static bool ContainsAll<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return first.ContainsAll(second, null);
        }

        public static bool ContainsAll<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> comparer)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");
            if (comparer == null)
                comparer = EqualityComparer<TValue>.Default;
            foreach (var kv in second)
            {
                TValue value;
                if (!first.TryGetValue(kv.Key, out value))
                    return false;
                if (!comparer.Equals(value, kv.Value))
                    return false;
            }
            return true;
        }

        public static bool ContainsAll<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
        {
            return first.ContainsAll<TKey, TValue>(second, null);
        }

        public static bool ContainsAll<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> first, IEnumerable<KeyValuePair<TKey, TValue>> second, IEqualityComparer<TValue> comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            IDictionary<TKey, TValue> firstAsDict = first as IDictionary<TKey, TValue>;
            IDictionary<TKey, TValue> secondAsDict = second as IDictionary<TKey, TValue>;
            if (first == null || second == null)
                return first.ContainsAll(second, new KeyValueComparator<TKey, TValue>(comparer));
            return firstAsDict.ContainsAll(second, comparer);
        }

        public static bool ContainsOnly<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
        {
            return first.ContainsOnly<TKey, TValue>(second, null);
        }

        public static bool ContainsOnly<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> first, IEnumerable<KeyValuePair<TKey, TValue>> second, IEqualityComparer<TValue> comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            IDictionary<TKey, TValue> firstAsDict = first as IDictionary<TKey, TValue>;
            IDictionary<TKey, TValue> secondAsDict = second as IDictionary<TKey, TValue>;
            if (first == null || second == null)
                return first.ContainsOnly(second, new KeyValueComparator<TKey, TValue>(comparer));
            return firstAsDict.ContainsOnly(second, comparer);
        }

        public static bool ContainsOnly<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            return first.ContainsOnly(second, null);
        }

        public static bool ContainsOnly<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (first.Count != second.Count)
                return false;
            return first.ContainsAll(second);
        }

        public static string ToString<T>(this IEnumerable<T> e, string separator)
        {
            if (separator == null)
                throw new ArgumentNullException();
            return ToString<T, string>(e, separator);
        }

        public static string ToString<T>(this IEnumerable<T> e, char separator)
        {
            return ToString<T, char>(e, separator);
        }

        internal static string ToString<T, V>(this IEnumerable<T> e, V separator)
        {
            if (e == null)
                throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            using (IEnumerator<T> enumerator = e.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    sb.Append(enumerator.Current);
                while (enumerator.MoveNext())
                    sb.Append(separator).Append(enumerator.Current);
            }
            return sb.ToString();
        }

        private class KeyValueComparator<TKey, TValue> : EqualityComparer<KeyValuePair<TKey, TValue>>
        {
            public IEqualityComparer<TKey> KeyComparer { get; private set; }
            public IEqualityComparer<TValue> ValueComparer { get; private set; }

            public KeyValueComparator()
                : this(null, null)
            {
            }

            public KeyValueComparator(IEqualityComparer<TKey> keyComp)
                : this(keyComp, null)
            {
            }

            public KeyValueComparator(IEqualityComparer<TValue> valueComp)
                : this(null, valueComp)
            {
            }

            public KeyValueComparator(IEqualityComparer<TKey> keyComp, IEqualityComparer<TValue> valueComp)
            {
                KeyComparer = keyComp == null ? EqualityComparer<TKey>.Default : keyComp;
                ValueComparer = valueComp == null ? EqualityComparer<TValue>.Default : valueComp;
            }

            public override bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return KeyComparer.Equals(x.Key, y.Key) && ValueComparer.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return KeyComparer.GetHashCode(obj.Key) ^ ValueComparer.GetHashCode(obj.Value);
            }
        }
    }
}