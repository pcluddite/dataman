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
    public static class ArrayExtensions
    {
        public static int[] GetLengths(this Array array)
        {
            int rank = array.Rank;
            int[] lens = new int[rank];
            for (int dim = 0; dim < rank; ++dim)
                lens[dim] = array.GetLength(dim);
            return lens;
        }

        public static T[] BlockCopy<T>(this T[] array)
        {
            T[] newArr = new T[array.Length];
            Buffer.BlockCopy(array, 0, newArr, 0, Buffer.ByteLength(newArr));
            return newArr;
        }

        public static Array BlockCopy(this Array array)
        {
            if (array.Rank == 1)
                return array.BlockCopy(array.Length);
            return array.BlockCopy(array.GetLengths());
        }

        public static Array BlockCopy(this Array array, params int[] lengths)
        {
            Array newArr = Array.CreateInstance(array.GetType().GetElementType(), lengths);
            Buffer.BlockCopy(array, 0, newArr, 0, Buffer.ByteLength(newArr));
            return newArr;
        }

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

        private static string ToString<T, V>(IEnumerable<T> e, V separator)
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

        private static string ToString<V>(IEnumerable e, V separator)
        {
            if (e == null)
                throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            IEnumerator enumerator = e.GetEnumerator();
            if (enumerator.MoveNext())
                sb.Append(enumerator.Current);
            while (enumerator.MoveNext())
                sb.Append(separator).Append(enumerator.Current);
            
            return sb.ToString();
        }

        public static string ArrayToString(this Array array, char separator)
        {
            return ArrayToString<char, char>(array, separator, separator);
        }

        public static string ArrayToString(this Array array, char separator, char groupSeparator)
        {
            return ArrayToString<char, char>(array, separator, groupSeparator);
        }

        public static string ArrayToString(this Array array, char separator, string groupSeparator)
        {
            return ArrayToString<char, string>(array, separator, groupSeparator);
        }

        public static string ArrayToString(this Array array, string separator)
        {
            return ArrayToString<string, string>(array, separator, separator);
        }

        public static string ArrayToString(this Array array, string separator, string groupSeparator)
        {
            return ArrayToString<string, string>(array, separator, groupSeparator);
        }

        public static string ArrayToString(this Array array, string separator, char groupSeparator)
        {
            return ArrayToString<string, char>(array, separator, groupSeparator);
        }

        private static string ArrayToString<T, V>(Array array, T separator, V groupSeparator)
        {
            if (array == null)
                throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            int rank = array.Rank;
            if (rank == 1)
            {
                sb.Append(ToString((IEnumerable)array, separator));
            }
            else
            {
                ArrayToString(sb, array, 0, new int[array.Rank], separator, groupSeparator);
            }
            return sb.ToString();
        }

        private static void ArrayToString<T, V>(StringBuilder sb, Array array, int dim, int[] indices, T separator, V groupSeparator)
        {
            for (int i = 0; i < array.GetLength(dim); ++i)
            {
                indices[dim] = i;
                if (dim == array.Rank - 1)
                {
                    if (i > 0)
                        sb.Append(separator);
                    sb.Append(array.GetValue(indices));
                }
                else
                {
                    sb.Append('{');
                    ArrayToString(sb, array, dim + 1, indices, separator, groupSeparator);
                    sb.Append('}');
                    if (indices[dim] < array.GetLength(dim) - 1)
                        sb.Append(groupSeparator);
                }
            }
        }
    }
}
