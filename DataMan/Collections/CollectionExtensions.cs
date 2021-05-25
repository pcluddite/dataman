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
using System.Reflection;
using System.Text;

namespace Baxendale.DataManagement.Collections
{
    public static class CollectionExtensions
    {
        private static readonly Random RandomInstance = new Random();

        public static void Add(this ICollection collection, object item)
        {
            Type collectionType = collection.GetType();
            MethodInfo addMethods = collectionType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(object) }, null);
            addMethods.Invoke(collection, new object[] { item });
        }

        public static bool? IsReadOnly(this ICollection collection)
        {
            Type collectionType = collection.GetType();
            try
            {
                PropertyInfo readOnlyProperty = collectionType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.Public, null, typeof(bool), new Type[0], null);
                if (readOnlyProperty == null)
                    return null;
                MethodInfo getMethod = readOnlyProperty.GetGetMethod(false);
                if (getMethod == null)
                    return null;
                return (bool)getMethod.Invoke(collection, new object[0]);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
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

        public static IEnumerable<TSource> Randomize<TSource>(this IEnumerable<TSource> e)
        {
            IList<TSource> options = new List<TSource>(e);
            while (options.Count > 0)
            {
                int idx = RandomInstance.Next(0, options.Count);
                yield return options[idx];
                options.RemoveAt(idx);
            }
        }

        public static bool ContainsOnly<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            LinkedList<TSource> secondList = new LinkedList<TSource>(second);
            foreach (TSource o1 in first)
            {
                int removed = 0;
                LinkedListNode<TSource> node = secondList.First;
                while(node != null)
                {
                    if (Equals(o1, node.Value))
                    {
                        ++removed;
                        secondList.Remove(node);
                    }
                    node = node.Next;
                }
                if (removed == 0)
                    return false;
            }
            return secondList.Count > 0;
        }

        public static IEnumerable<char> AlphaSequence(this char startChar)
        {
            if (char.IsLetter(startChar))
            {
                if (char.IsLower(startChar))
                {
                    return startChar.Sequence('z' - startChar + 1);
                }
                else
                {
                    return startChar.Sequence('Z' - startChar + 1);
                }
            }
            else if (char.IsDigit(startChar))
            {
                return startChar.Sequence('9' - startChar + 1);
            }
            throw new ArgumentException("Character must be a number or a letter to sequence");
        }

        public static IEnumerable<char> Sequence(this char startChar, int count)
        {
            for (int i = 0; i < startChar; ++i)
                yield return (char)(startChar + i);
        }
    }
}
