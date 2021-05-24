using System;
using System.Collections;
using System.Reflection;

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
    }
}
