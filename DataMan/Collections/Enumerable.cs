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

namespace Baxendale.DataManagement.Collections
{
    public static class Enumerable
    {
        private static readonly Random RandomInstance = new Random();
        private static readonly object _object = new object();

        public static IEnumerable<char> AlphaSequence(this char startChar)
        {
            if (char.IsLetter(startChar))
            {
                if (char.IsLower(startChar))
                {
                    return startChar.AlphaSequence('z' - startChar + 1);
                }
                else
                {
                    return startChar.AlphaSequence('Z' - startChar + 1);
                }
            }
            else if (char.IsDigit(startChar))
            {
                return startChar.AlphaSequence('9' - startChar + 1);
            }
            throw new ArgumentException("Character must be a number or a letter to sequence", nameof(startChar));
        }

        public static IEnumerable<char> AlphaSequence(this char startChar, int count)
        {
            for (int i = 0; i < startChar; ++i)
                yield return (char)(startChar + i);
        }

        public static bool ContainsAll<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.ContainsAll(second, null);
        }

        public static bool ContainsAll<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            ISet<TSource> set = new HashSet<TSource>(first, comparer);
            foreach (TSource item in second)
                if (!set.Contains(item)) return false;
            return true;
        }

        public static bool ContainsOnly<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.ContainsOnly(second, null);
        }

        public static bool ContainsOnly<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            ISet<TSource> firstSet = new HashSet<TSource>(first, comparer);
            ISet<TSource> secondSet = new HashSet<TSource>(second, comparer);
            if (firstSet.Count != secondSet.Count)
                return false;
            firstSet.IntersectWith(secondSet);
            return firstSet.Count == secondSet.Count;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collections)
        {
            foreach (IEnumerable<T> collection in collections)
            {
                foreach (T item in collection)
                    yield return item;
            }
        }

        public static IEnumerable<T> Singleton<T>(this T o)
        {
            yield return o;
        }

        public static IEnumerable<TSource> Randomize<TSource>(this IEnumerable<TSource> e)
        {
            IList<TSource> options = new List<TSource>(e);
            while (options.Count > 0)
            {
                lock (_object)
                {
                    int idx = RandomInstance.Next(0, options.Count);
                    yield return options[idx];
                    options.RemoveAt(idx);
                }
            }
        }
    }
}
