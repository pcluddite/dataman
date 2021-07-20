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
            if (currentType == null) throw new ArgumentNullException(nameof(currentType));
            while ((currentType = currentType.BaseType) != null && currentType != typeof(object))
                yield return currentType;
        }

        public static IEnumerable<Type> GetHierarchy(this Type currentType)
        {
            if (currentType == null) throw new ArgumentNullException(nameof(currentType));

            yield return currentType;

            while ((currentType = currentType.BaseType) != null && currentType != typeof(object))
                yield return currentType;
        }

        public static Dictionary<Type, ISet<Type>> GetDeclaredInterfaces(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Dictionary<Type, ISet<Type>> interfaces = new Dictionary<Type, ISet<Type>>();
            HashSet<Type> currentSet = new HashSet<Type>(type.GetInterfaces());
            
            interfaces.Add(type, currentSet);

            foreach (Type parent in type.GetBaseTypes())
            {
                HashSet<Type> parentSet = new HashSet<Type>(parent.GetInterfaces());
                currentSet.ExceptWith(parentSet);
                interfaces.Add(parent, parentSet);
                currentSet = parentSet;
            }

            return interfaces;
        }

        public static bool IsUnboundGeneric(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Type[] genericArguments = type.GetGenericArguments();
            return genericArguments == null || genericArguments.Length == 0 || ((genericArguments[0].Attributes & TypeAttributes.BeforeFieldInit) != TypeAttributes.BeforeFieldInit);
        }

        public static Type GetUnboundType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        public static bool HasSameGenerics(this Type a, Type b)
        {
            Type[] aGenerics = b.GetGenericArguments();
            Type[] bGenerics = a.GetGenericArguments();
            if (aGenerics.Length != bGenerics.Length)
                return false;
            for(int idx = 0; idx < aGenerics.Length; ++idx)
            {
                if (aGenerics[idx] != bGenerics[idx] && !bGenerics[idx].IsSubclassOf(aGenerics[idx]))
                    return false;
            }
            return true;
        }

        public static Type GetGenericInterfaceDefinition(this Type type, Type interfaceType)
        {
            bool isUnboundInterface = IsUnboundGeneric(interfaceType);
            Type unboundInterface = GetUnboundType(interfaceType);

            do
            {
                Type unboundType = GetUnboundType(type);
                if (interfaceType == unboundType)
                    return type;
                foreach (Type definedInterface in type.GetInterfaces().Where(i => GetUnboundType(i) == unboundInterface))
                {
                    if (isUnboundInterface || HasSameGenerics(definedInterface, interfaceType))
                        return definedInterface;
                }
            }
            while ((type = type.BaseType) != null);
            return null;
        }
    }
}
