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
using System.Linq;

namespace Baxendale.DataManagement.Collections
{
    public static class Arrays
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

        internal static string ArrayToString<T, V>(Array array, T separator, V groupSeparator)
        {
            if (array == null)
                throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            int rank = array.Rank;
            if (rank == 1)
            {
                sb.Append(array.Cast<object>().ToString(separator));
            }
            else
            {
                ArrayToString(sb, array, 0, new int[rank], separator, groupSeparator);
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
