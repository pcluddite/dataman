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
using System.Linq;
using System.Reflection;

namespace Baxendale.Data.Reflection
{
    public static class ReflectionExtensions
    {
        public static T Convert<T>(this IConvertible c) where T : IConvertible
        {
            return (T)System.Convert.ChangeType(c, typeof(T));
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
                    throw new TargetException($"{info.Name} in {info.DeclaringType.Name} does not have a value that can be set");
            }
        }

        public static object GetValue(this MemberInfo info, object obj)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo)info).GetValue(obj);
                case MemberTypes.Property: return ((PropertyInfo)info).GetGetMethod(nonPublic: true).Invoke(obj, new object[0]);
                default:
                    throw new TargetException($"{info.Name} in {info.DeclaringType.Name} does not have a value that can be gotten");
            }
        }

        public static T GetValue<T>(this MemberInfo info, T obj)
        {
            object value = info.GetValue((object)obj);
            if (value is T)
                return (T)value;
            if (!(value is IConvertible) || !typeof(IConvertible).IsAssignableFrom(typeof(T)))
                throw new InvalidCastException();
            return (T)System.Convert.ChangeType(value, typeof(T));
        }

        public static Type GetReturnType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo)info).FieldType;
                case MemberTypes.Property: return ((PropertyInfo)info).PropertyType;
                case MemberTypes.Method: return ((MethodInfo)info).ReturnType;
                default:
                    throw new TargetException($"{info.Name} in {info.DeclaringType.Name} does not have a return type");
            }
        }

        public static IEnumerable<Type> GetBaseTypes(this Type currentType)
        {
            while ((currentType = currentType.BaseType) != null)
                yield return currentType;
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
