using System;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace Baxendale.DataManagement.Reflection
{
    public static class ReflectionExtensions
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

        // Adapted from https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class/18828085#18828085

        public static Type GetGenericBaseType(this Type type, Type interfaceType)
        {
            Type[] typeParamters = interfaceType.GetGenericArguments();
            bool isParameterLessGeneric = !(typeParamters != null && typeParamters.Length > 0 &&
                ((typeParamters[0].Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit));
            do
            {
                Type cur = GetFullTypeDefinition(type);
                if (interfaceType == cur || (isParameterLessGeneric && cur.GetInterfaces().Select(i => GetFullTypeDefinition(i)).Contains(GetFullTypeDefinition(interfaceType))))
                {
                    return type;
                }
                else if (!isParameterLessGeneric)
                {
                    foreach (Type item in type.GetInterfaces().Where(i => GetFullTypeDefinition(interfaceType) == GetFullTypeDefinition(i)))
                    {
                        if (VerifyGenericArguments(interfaceType, item)) return item;
                    }
                }
                type = type.BaseType;
            }
            while (type != null && type != typeof(object));
            return null;
        }

        // Stolen from https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class/18828085#18828085

        public static bool IsSubClassOfGeneric(this Type child, Type parent)
        {
            if (child == parent)
                return false;

            if (child.IsSubclassOf(parent))
                return true;

            var parameters = parent.GetGenericArguments();
            var isParameterLessGeneric = !(parameters != null && parameters.Length > 0 &&
                ((parameters[0].Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit));

            while (child != null && child != typeof(object))
            {
                var cur = GetFullTypeDefinition(child);
                if (parent == cur || (isParameterLessGeneric && cur.GetInterfaces().Select(i => GetFullTypeDefinition(i)).Contains(GetFullTypeDefinition(parent))))
                    return true;
                else if (!isParameterLessGeneric)
                    if (GetFullTypeDefinition(parent) == cur && !cur.IsInterface)
                    {
                        if (VerifyGenericArguments(GetFullTypeDefinition(parent), cur))
                            if (VerifyGenericArguments(parent, child))
                                return true;
                    }
                    else
                        foreach (var item in child.GetInterfaces().Where(i => GetFullTypeDefinition(parent) == GetFullTypeDefinition(i)))
                            if (VerifyGenericArguments(parent, item))
                                return true;

                child = child.BaseType;
            }

            return false;
        }

        public static Type GetFullTypeDefinition(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        public static bool VerifyGenericArguments(this Type parent, Type child)
        {
            Type[] childArguments = child.GetGenericArguments();
            Type[] parentArguments = parent.GetGenericArguments();
            if (childArguments.Length == parentArguments.Length)
                for (int i = 0; i < childArguments.Length; i++)
                    if (childArguments[i].Assembly != parentArguments[i].Assembly || childArguments[i].Name != parentArguments[i].Name || childArguments[i].Namespace != parentArguments[i].Namespace)
                        if (!childArguments[i].IsSubclassOf(parentArguments[i]))
                            return false;

            return true;
        }
    }
}
