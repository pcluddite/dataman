using System;
using System.Collections;
using System.Reflection;

namespace VirtualFlashCards.Xml
{
    internal static class ReflectionExtensions
    {
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
    }
}
