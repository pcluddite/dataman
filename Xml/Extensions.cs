using System;
using System.Reflection;
using System.Xml;
using System.Collections;

namespace VirtualFlashCards.Xml
{
    internal static class Extensions
    {
        public static XmlAttribute Attributes(this XmlNode node, string name)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(name);
                attr.Value = "";
                node.Attributes.Append(attr);
            }
            return attr;
        }

        public static T Value<T>(this XmlAttribute attr) where T : IConvertible
        {
            return (T)System.Convert.ChangeType(attr.Value, typeof(T));
        }

        public static T Value<T>(this XmlAttribute attr, T @default) where T : IConvertible
        {
            T value;
            if (attr.TryValue(out value))
                return value;
            return @default;
        }

        public static bool TryValue<T>(this XmlAttribute attr, out T value) where T : IConvertible
        {
            try
            {
                value = attr.Value<T>();
                return true;
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidCastException || ex is FormatException))
                    throw;
                value = default(T);
                return false;
                
            }
        }

        public static T Convert<T>(this IConvertible c) where T : IConvertible
        {
            return (T)System.Convert.ChangeType(c, typeof(T));
        }

        public static bool IsCollection(this Type t)
        {
            return t.IsArray || typeof(IEnumerable).IsAssignableFrom(t);
        }

        public static object CreateDefault(this Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }

        public static void SetValue(this MemberInfo info, object obj, object value)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field: ((FieldInfo)info).SetValue(obj, value); break;
                case MemberTypes.Property: ((PropertyInfo)info).SetValue(obj, value, null); break;
                default:
                    throw new TargetException(info.Name + " in " + info.DeclaringType.Name + " does not have a value that can be set");
            }
        }

        public static Type GetReturnType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo)info).FieldType;
                case MemberTypes.Property: return ((PropertyInfo)info).PropertyType;
                case MemberTypes.Method: return ((MethodInfo)info).ReturnType;
                default:
                    throw new TargetException(info.Name + " in " + info.DeclaringType.Name + " does not have a return type");
            }
        }

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
    }
}
